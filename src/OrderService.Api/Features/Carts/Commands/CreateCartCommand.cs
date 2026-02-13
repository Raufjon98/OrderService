using CatalogService.Contracts.Food.Requests;
using CatalogService.Contracts.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Constants;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Features.Common.Exceptions;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Cart.Events;
using OrderService.Contracts.Cart.Requests;
using OrderService.Contracts.Cart.Responses;
using OrderService.Contracts.CartItem.Responses;

namespace OrderService.Api.Features.Carts.Commands;

public record CreateCartCommand(Guid CustomerId, CreateCartRequest CartRequest) : IRequest<CartResponse>;

public class CreateCartCommandHandler : IRequestHandler<CreateCartCommand, CartResponse>
{
    private readonly OrderDbContext _context;
    private readonly IFoodService _foodService;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateCartCommandHandler(
        OrderDbContext context,
        IFoodService foodService, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _foodService = foodService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CartResponse> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var existingCart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c=>c.CustomerId == request.CustomerId, cancellationToken);

        if (existingCart != null)
        {
            return new CartResponse
            {
                Id = existingCart.Id,
                CustomerId = existingCart.CustomerId,
                Items = existingCart.Items.Select(i => new CartItemResponse
                {
                    CartId = existingCart.Id,
                    Quantity = i.Quantity,
                    FoodId = i.FoodId,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }

        {
            var cart = new Cart
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
            };

            decimal totalPrice = 0;
            int quantity = 0;

            var items = request.CartRequest.Items;
            string foodId = request.CartRequest.Items.First().FoodId;
            var foodtmp = await _foodService.GetFoodAsync(foodId);
            var restaurantId = foodtmp.RestaurantId;

            foreach (var item in items)
            {
                if (item.Quantity == 0)
                {
                    throw new Exception("Quantity cannot be 0");
                }

                var food = await _foodService.GetFoodAsync(item.FoodId);
                if (food == null)
                {
                    throw new NotFoundException(FoodConstant.FoodName, item.FoodId);
                }

                if (food.RestaurantId != restaurantId)
                {
                    throw new Exception($"You couldn't add food from several restaurants!");
                }

                if (food.Stock < item.Quantity)
                {
                    throw new Exception($"Available food quantity {food.Stock} is less than {item.Quantity}");
                }

                CartItem cartItem = new CartItem
                {
                    CartId = cart.Id,
                    FoodId = item.FoodId,
                    Quantity = item.Quantity,
                    UnitPrice = food.Price,
                };

                cart.Items.Add(cartItem);
                totalPrice += cartItem.Subtotal;
                quantity += item.Quantity;
            }

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(
                new CartCreatedEvent
                {
                    Id = cart.Id,
                    ItemsIds =  cart.Items.Select(i => i.Id).ToArray(),
                    CreatedOnUtc = DateTime.UtcNow,
                },
                cancellationToken);

            var foodstockRequest = cart.Items.Select(i => new FoodStockRequest
                {
                    FoodId = i.FoodId,
                    Quantity = i.Quantity
                }
            ).ToList();

            await _foodService.DecreaseFoodStockAsync(foodstockRequest);

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
}