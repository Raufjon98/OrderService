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

public record AddItemToCartCommand(Guid CustomerId, CartItemsRequest CartItems) : IRequest<CartResponse>;

public class AddItemToCartCommandHandler : IRequestHandler<AddItemToCartCommand, CartResponse>
{
    private readonly OrderDbContext _context;
    private readonly IFoodService _foodService;
    private readonly IPublishEndpoint _publishEndpoint;

    public AddItemToCartCommandHandler(OrderDbContext context,
        IFoodService foodService, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _foodService = foodService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CartResponse> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
    {
        var updateFoodStock = new List<FoodStockRequest>();

        if (!request.CartItems.Items.Any())
        {
            throw new Exception("Cart items are empty");
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .Where(c => c.CustomerId == request.CustomerId && c.IsDeleted == false)
            .FirstOrDefaultAsync(cancellationToken);

        string? restaurantId = null;

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (cart is null)
            {
                var cartId = Guid.NewGuid();
                cart = new Cart
                {
                    Id = cartId,
                    CustomerId = request.CustomerId,
                };
                foreach (var cartItem in request.CartItems.Items)
                {
                    var food = await _foodService.GetFoodAsync(cartItem.FoodId);
                    if (food is null)
                    {
                        throw new NotFoundException(FoodConstant.FoodName, cartItem.FoodId);
                    }

                    if (food.Stock < cartItem.Quantity)
                    {
                        throw new Exception($"Available food quantity {food.Stock} is less than {cartItem.Quantity}");
                    }

                    if (restaurantId == null)
                    {
                        restaurantId = food.RestaurantId;
                    }
                    else if (restaurantId != food.RestaurantId)
                    {
                        throw new Exception($"You couldn't add food from several restaurants!");
                    }

                    var item = new CartItem
                    {
                        CartId = cartId,
                        FoodId = cartItem.FoodId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = food.Price
                    };

                    cart.Items.Add(item);
                    await _context.CartItems.AddAsync(item);

                    updateFoodStock.Add(new FoodStockRequest()
                    {
                        FoodId = cartItem.FoodId,
                        Quantity = cartItem.Quantity
                    });
                }

                await _foodService.DecreaseFoodStockAsync(updateFoodStock);
                await _context.Carts.AddAsync(cart, cancellationToken);
                await _publishEndpoint.Publish(
                    new CartCreatedEvent
                    {
                        Id = cartId,
                        ItemsIds = cart.Items.Select(i => i.Id).ToArray(),
                        CreatedOnUtc = DateTime.UtcNow
                    },
                    cancellationToken);
            }
            else
            {
                if (cart.Items.Any())
                {
                    var existingFood = await _foodService.GetFoodAsync(cart.Items.First().FoodId);
                    if (existingFood is null)
                    {
                        throw new NotFoundException(FoodConstant.FoodName, cart.Items.First().FoodId);
                    }

                    restaurantId = existingFood.RestaurantId;
                }

                var cartUpdatedEvent = new CartUpdatedEvent
                {
                    Id = cart.Id,
                    Source = "AddItemToCart",
                    UpdatedOnUtc = DateTime.UtcNow
                };
                foreach (var item in request.CartItems.Items)
                {
                    var food = await _foodService.GetFoodAsync(item.FoodId);
                    if (food is null)
                    {
                        throw new NotFoundException(FoodConstant.FoodName, item.FoodId);
                    }

                    if (restaurantId == null)
                    {
                        restaurantId = food.RestaurantId;
                    }
                    else if (restaurantId != food.RestaurantId)
                    {
                        throw new Exception($"You couldn't add food from several restaurants!");
                    }

                    var existingItem = cart.Items.FirstOrDefault(i => i.FoodId == item.FoodId);

                    if (existingItem is not null)
                    {
                        int finalStock = existingItem.Quantity + item.Quantity;
                        if (finalStock > food.Stock)
                        {
                            throw new Exception($"Stock {food.Stock} is less than {finalStock}");
                        }

                        existingItem.Quantity = finalStock;

                        updateFoodStock.Add(new FoodStockRequest()
                        {
                            FoodId = item.FoodId,
                            Quantity = item.Quantity
                        });
                    }
                    else
                    {
                        if (item.Quantity > food.Stock)
                        {
                            throw new Exception($"Stock {food.Stock} is less than {item.Quantity}");
                        }

                        var cartItem = new CartItem
                        {
                            Id = Guid.NewGuid(),
                            CartId = cart.Id,
                            FoodId = item.FoodId,
                            Quantity = item.Quantity,
                            UnitPrice = food.Price
                        };

                        updateFoodStock.Add(new FoodStockRequest()
                        {
                            FoodId = item.FoodId,
                            Quantity = item.Quantity
                        });

                        cart.Items.Add(cartItem);
                        await _context.CartItems.AddAsync(cartItem, cancellationToken);
                        cartUpdatedEvent.FoodIds.Add(item.FoodId);
                    }
                }
               
                await _publishEndpoint.Publish(cartUpdatedEvent, cancellationToken);
                await _foodService.DecreaseFoodStockAsync(updateFoodStock);
            }

            await _context.SaveChangesAsync(cancellationToken);
           
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new Exception("Error while adding to cart", e);
        }

        return new CartResponse
        {
            Id = cart.Id,
            CustomerId = cart.CustomerId,
            Items = cart.Items.Select(c =>
                new CartItemResponse
                {
                    CartId = cart.Id,
                    FoodId = c.FoodId,
                    UnitPrice = c.UnitPrice,
                    Quantity = c.Quantity
                }).ToList()
        };
    }
}