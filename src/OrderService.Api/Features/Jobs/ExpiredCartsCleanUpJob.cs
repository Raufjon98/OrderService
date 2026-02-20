using CatalogService.Contracts.Food.Requests;
using CatalogService.Contracts.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Cart.Events;

namespace OrderService.Api.Features.Jobs;

public interface IExpiredCartsCleanUpJob
{
    Task RemoveExpiredCartsAsync(CancellationToken cancellationToken);
}

public class ExpiredCartsCleanUpJob : IExpiredCartsCleanUpJob
{
    private readonly ILogger<ExpiredCartsCleanUpJob> _logger;
    private readonly OrderDbContext _context;
    private readonly IFoodService _foodService;
    private readonly IPublishEndpoint _publishEndpoint;
    
    public ExpiredCartsCleanUpJob(ILogger<ExpiredCartsCleanUpJob> logger,
        OrderDbContext context,
        IPublishEndpoint publishEndpoint,
        IFoodService foodService)
    {
        _logger = logger;
        _context = context;
        _publishEndpoint = publishEndpoint;
        _foodService = foodService;
    }
    
    public async Task RemoveExpiredCartsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var expiredCarts = await _context.Carts
                .Where(c => c.CreatedAt < DateTime.UtcNow 
                && c.Items.All(i => i.Quantity > 0))
                .ToListAsync(cancellationToken);
            
            foreach (var cart in expiredCarts)
            {
                var updateFoodStock = new List<FoodStockRequest>();
                var cartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync(cancellationToken);
                
                foreach (var cartItem in cartItems)
                {
                    cartItem.IsDeleted = true;
                    updateFoodStock.Add(new  FoodStockRequest
                    {
                        FoodId = cartItem.FoodId,
                        Quantity = cartItem.Quantity
                    });
                }
                
                await _foodService.IncreaseFoodStockAsync(updateFoodStock);
                cart.IsDeleted = true;
                await _publishEndpoint.Publish(
                    new CartRemovedEvent
                    {
                        CustomerId = cart.CustomerId,
                        RemovedOnUtc = DateTime.UtcNow
                    },
                    cancellationToken);
                
                _logger.LogInformation($"Removed {cart.Id} expired carts");
            }
            
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }    
    }
}