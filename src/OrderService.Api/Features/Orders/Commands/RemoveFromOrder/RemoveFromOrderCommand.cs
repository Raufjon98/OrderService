using CatalogService.Contracts.Food.Requests;
using CatalogService.Contracts.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Features.Common.Exceptions;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Interfaces;
using OrderService.Contracts.Order.Events;
using OrderService.Contracts.Order.Requests;
using OrderService.Contracts.Order.Responses;
using OrderService.Contracts.OrderItem.Responses;
using PaymentService.Contracts.Account.Requests;
using PaymentService.Contracts.Interfaces;

namespace OrderService.Api.Features.Orders.Commands.RemoveFromOrder;

public record RemoveFromOrderCommand(Guid CustomerId, RemoveFromOrderRequest RemoveFromOrderRequest) : IRequest<OrderResponse>;

public class RemoveFromOrderCommandHandler : IRequestHandler<RemoveFromOrderCommand, OrderResponse>
{
    private readonly IFoodService _foodService;
    private readonly OrderDbContext _context;
    private readonly IAccountService _accountService;
    private readonly IPublishEndpoint _publishEndpoint;

    public RemoveFromOrderCommandHandler(IFoodService foodService,
        OrderDbContext context,
        IAccountService accountService, IPublishEndpoint publishEndpoint)
    {
        _foodService = foodService;
        _context = context;
        _accountService = accountService;
        _publishEndpoint = publishEndpoint;
    }
    
    public async Task<OrderResponse> Handle(RemoveFromOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o=>o.Items)
            .Where(o => o.CustomerId == request.CustomerId && o.Id == request.RemoveFromOrderRequest.OrderId)
            .FirstOrDefaultAsync(cancellationToken);
        
        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.RemoveFromOrderRequest.OrderId.ToString());
        }

        if (order.Status is OrderStatus.Pending or OrderStatus.Confirmed or OrderStatus.Preparing)
        {
            decimal amount = 0;
            var updateFoodsStock = new List<FoodStockRequest>();
            
            var orderUpdatedEvent = new OrderUpdatedEvent
            {
                Id = order.Id,
                OrderStatus = order.Status,
                Source = "RemoveFromOrder",
                UpdatedOnUtc = DateTime.UtcNow,
            };

            foreach (var item in  request.RemoveFromOrderRequest.Items)
            {
                var existingItem = order.Items.FirstOrDefault(i => i.FoodId == item.FoodId);
                if (existingItem is null)
                {
                    throw new Exception($"Order doesn't contains item with key:{item.FoodId}");
                }

                if (existingItem.Quantity < item.Quantity)
                {
                    throw new Exception("Removing item doesn't have enough quantity");
                }
            
                existingItem.Quantity -= item.Quantity;
                amount += existingItem.UnitPrice * item.Quantity;
            
                var updatefoodStock = new FoodStockRequest
                {
                    FoodId = item.FoodId,
                    Quantity = item.Quantity
                };
                updateFoodsStock.Add(updatefoodStock);
                 
                if (existingItem.Quantity == 0)
                {
                    order.Items.Remove(existingItem);
                }
                orderUpdatedEvent.Items.Add(item.FoodId, item.Quantity);
            }
            
            var refund = new TopUpRequest
            {
                CustomerId = request.CustomerId,
                Amount = amount,
                SourceId = order.Id.ToString()
            };
            await _accountService.TopUpBalanceAsync(refund);
            
            if (!order.Items.Any())
            {
                order.IsDeleted = true;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync(cancellationToken);
            }
            
            await _foodService.IncreaseFoodStockAsync(updateFoodsStock);
            await _context.SaveChangesAsync(cancellationToken);
            await _publishEndpoint.Publish(orderUpdatedEvent, cancellationToken);
        }
        else
        {
            throw new Exception("Your order already cooked!");
        }
        
        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            OrderDate = order.CreatedAt,
            Status = order.Status,
            Items = order.Items.Select(o=>  new OrderItemResponse
            {
                OrderId = o.OrderId,
                FoodId = o.FoodId,
                Quantity = o.Quantity,
                UnitPrice = o.UnitPrice,
            }).ToList()
        };
    }
}