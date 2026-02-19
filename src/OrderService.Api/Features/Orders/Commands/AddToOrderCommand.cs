using CatalogService.Contracts.Food.Requests;
using CatalogService.Contracts.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Constants;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Features.Common.Exceptions;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Order.Events;
using OrderService.Contracts.Order.Requests;
using OrderService.Contracts.Order.Responses;
using OrderService.Contracts.OrderItem.Responses;
using PaymentService.Contracts.Account.Requests;
using PaymentService.Contracts.Interfaces;

namespace OrderService.Api.Features.Orders.Commands;

public record AddToOrderCommand(Guid CustomerId, AddToOrderRequest AddToOrderRequest) : IRequest<OrderResponse>;

public class AddToOrderCommandHandler : IRequestHandler<AddToOrderCommand, OrderResponse>
{
    private readonly OrderDbContext _context;
    private readonly IFoodService _foodService;
    private readonly IAccountService _accountService;
    private readonly IPublishEndpoint _publishEndpoint;

    public AddToOrderCommandHandler(OrderDbContext context,
        IFoodService foodService,
        IAccountService accountService, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _foodService = foodService;
        _accountService = accountService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<OrderResponse> Handle(AddToOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == request.CustomerId
                        && o.Id == request.AddToOrderRequest.OrderId)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (order == null)
        {   
            throw new NotFoundException(nameof(Order), request.AddToOrderRequest.OrderId.ToString());
        }

        string? orderRestaurantId = null;
        if (order.Items.Any())
        {
            var existingFood = await _foodService.GetFoodAsync(order.Items.First().FoodId);
            if (existingFood == null)
            {
                throw new NotFoundException(FoodConstant.FoodName,  order.Items.First().FoodId);
            }
            orderRestaurantId = existingFood.RestaurantId;
        }

        var updateFoodsStock = new List<FoodStockRequest>();
        var groupedAddtoOrderRequest = request.AddToOrderRequest
            .Items
            .GroupBy(i => i.FoodId)
            .Select(i => new { FoodId = i.Key, Quantity = i.Sum(gi => gi.Quantity) })
            .ToList();

        decimal amount = 0;
        var orderUpdatedEvent = new OrderUpdatedEvent
        {
            Id = order.Id,
            UpdatedOnUtc = DateTime.UtcNow,
            Source = "AddItemToOrder",
            OrderStatus = order.Status,
        };

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var newItem in groupedAddtoOrderRequest)
            {
                var food = await _foodService.GetFoodAsync(newItem.FoodId);
                if (food == null)
                {
                    throw new NotFoundException(FoodConstant.FoodName, newItem.FoodId.ToString());
                }

                if (orderRestaurantId == null)
                {
                    orderRestaurantId = food.RestaurantId;
                }
                else if (orderRestaurantId != food.RestaurantId)
                {
                    throw new Exception($"You couldn't add food from several restaurants!");
                }

                if (food.Stock < newItem.Quantity)
                {
                    throw new Exception($"Available food quantity {food.Stock} is less than {newItem.Quantity}");
                }

                amount += food.Price * newItem.Quantity;
                var existing = order.Items.FirstOrDefault(o => o.FoodId == newItem.FoodId);

                if (existing == null)
                {
                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        FoodId = newItem.FoodId,
                        Quantity = newItem.Quantity,
                        UnitPrice = food.Price,
                    };
                    order.Items.Add(orderItem);
                    await _context.OrderItems.AddAsync(orderItem, cancellationToken);
                }
                else
                {
                    existing.Quantity += newItem.Quantity;
                }

                var foodStockRequest = new FoodStockRequest
                {
                    FoodId = newItem.FoodId,
                    Quantity = newItem.Quantity
                };
                updateFoodsStock.Add(foodStockRequest);
                orderUpdatedEvent.Items.Add(newItem.FoodId, newItem.Quantity);
            }

            await _accountService.WithdrawBalanceAsync(new WithdrawRequest
            {
                CustomerId = request.CustomerId,
                Amount = amount,
                SourceId = order.OrderNumber.ToString(),
            });
            await _foodService.DecreaseFoodStockAsync(updateFoodsStock);
            await _context.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(orderUpdatedEvent, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch 
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            OrderDate = order.CreatedAt,
            Status = order.Status,
            Items = order.Items.Select(o => new OrderItemResponse
            {
                OrderId = o.OrderId,
                FoodId = o.FoodId,
                Quantity = o.Quantity,
                UnitPrice = o.UnitPrice,
            }).ToList()
        };
    }
}