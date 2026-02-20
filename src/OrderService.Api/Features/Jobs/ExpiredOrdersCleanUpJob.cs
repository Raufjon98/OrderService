using CatalogService.Contracts.Food.Requests;
using CatalogService.Contracts.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Interfaces;
using OrderService.Contracts.Order.Events;

namespace OrderService.Api.Features.Jobs;

public interface IExpiredOrdersCleanUpJob
{
    Task RemoveExpiredOrdersAsync(CancellationToken cancellationToken);
}

public class ExpiredOrdersCleanUpJob : IExpiredOrdersCleanUpJob
{
    private readonly OrderDbContext _orderDbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IFoodService _foodService;

    public ExpiredOrdersCleanUpJob(OrderDbContext orderDbContext, IPublishEndpoint publishEndpoint,
        IFoodService foodService)
    {
        _orderDbContext = orderDbContext;
        _publishEndpoint = publishEndpoint;
        _foodService = foodService;
    }

    public async Task RemoveExpiredOrdersAsync(CancellationToken cancellationToken)
    {
        try
        {
            var expiredOrders = await _orderDbContext.Orders
                .Where(o => o.CreatedAt < DateTime.UtcNow &&
                            o.Status == OrderStatus.PendingPayment &&
                            o.Items.All(i => i.Quantity > 0))
                .ToListAsync(cancellationToken);

            foreach (var order in expiredOrders)
            {
                var updateFoodStock = new List<FoodStockRequest>();
                var orderItems = await _orderDbContext.OrderItems
                    .Where(i => i.OrderId == order.Id)
                    .ToListAsync(cancellationToken);

                foreach (var orderItem in orderItems)
                {
                    orderItem.IsDeleted = true;
                    updateFoodStock.Add(new FoodStockRequest
                    {
                        FoodId = orderItem.FoodId,
                        Quantity = orderItem.Quantity
                    });
                }

                await _foodService.IncreaseFoodStockAsync(updateFoodStock);
                order.IsDeleted = true;
                
                await _publishEndpoint.Publish(
                    new OrderUpdatedEvent
                    {
                        Id = order.Id,
                        UpdatedOnUtc = DateTime.UtcNow,
                        OrderStatus = order.Status,
                        Source = "ExpiredOrdersCleanUpJob",
                    },
                    cancellationToken);
            } 
            await _orderDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}