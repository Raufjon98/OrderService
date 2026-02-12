using CatalogService.Contracts.Food.Requests;
using CatalogService.Contracts.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Features.Common.Exceptions;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Cart.Requests;
using OrderService.Contracts.Cart.Responses;
using OrderService.Contracts.CartItem.Responses;

namespace OrderService.Api.Features.Carts.Commands;

public record RemoveItemFromCartCommand(Guid CustomerId, CartItemsRequest CartItems) : IRequest<CartResponse>;

public class RemoveFromCartCommandHandler : IRequestHandler<RemoveItemFromCartCommand, CartResponse>
{
    private readonly OrderDbContext _context;
    private readonly IFoodService _foodService;

    public RemoveFromCartCommandHandler(OrderDbContext context,
        IFoodService foodService)
    {
        _context = context;
        _foodService = foodService;
    }
    public async Task<CartResponse> Handle(RemoveItemFromCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId
                && c.IsDeleted == false, cancellationToken);
        if (cart is null)
        {
          throw new NotFoundException(nameof(Cart), request.CustomerId.ToString());
        }

        var updateFoodStock = new List<FoodStockRequest>();

        foreach (var cartItem in request.CartItems.Items)
        {
            var item = cart.Items.FirstOrDefault(c=>c.FoodId == cartItem.FoodId);
            if (item is null)
            {
                throw new Exception($"You do not have item {cartItem.FoodId} for this cart!");
            }
            
            if (item.Quantity < cartItem.Quantity)
            {
                throw new Exception("not enough quantity in cart!");
            }
        
            if (item.Quantity == cartItem.Quantity)
            {
                cart.Items.Remove(item);
            }
        
            item.Quantity -= cartItem.Quantity;
        }
        
        await _foodService.IncreaseFoodStockAsync(updateFoodStock);
        
        if (!cart.Items.Any())
        {
            cart.IsDeleted = true;
        }
        await _context.SaveChangesAsync(cancellationToken);

        return new CartResponse
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Items = cart.Items.Select(c => new CartItemResponse
            {
                CartId = cart.Id,
                UnitPrice = c.UnitPrice,
                FoodId = c.FoodId,
                Quantity = c.Quantity,
            }).ToList()
        };
    }
}