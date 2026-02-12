using CatalogService.Contracts.Food.Requests;
using CatalogService.Contracts.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Features.Common.Exceptions;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Order.Events;
using OrderService.Contracts.Order.Responses;
using OrderService.Contracts.OrderItem.Responses;
using PaymentService.Contracts.Account.Requests;
using PaymentService.Contracts.Interfaces;

namespace OrderService.Api.Features.Orders.Commands;

public record CancelOrderCommand(Guid CustomerId, Guid OrderId) : IRequest<OrderResponse>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, OrderResponse>
{
    private readonly OrderDbContext _context;
    private readonly IAccountService _accountService;
    private readonly IFoodService _foodService;
    private readonly IPublishEndpoint _publishEndpoint;

    public CancelOrderCommandHandler(OrderDbContext context,
        IAccountService accountService,
        IFoodService foodService, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _accountService = accountService;
        _foodService = foodService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<OrderResponse> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId.ToString());
        }

        if (order.Status is OrderStatus.Pending or OrderStatus.Confirmed)
        {
            order.Status = OrderStatus.Cancelled;
            order.ModifiedAt = DateTime.UtcNow;
            var updateFoodsStock = order.Items
                .Select(i => new FoodStockRequest
                {
                    FoodId = i.FoodId,
                    Quantity = i.Quantity
                }).ToList();

            await _foodService.IncreaseFoodStockAsync(updateFoodsStock);
            
            await _publishEndpoint.Publish(
                new OrderUpdatedEvent()
                {
                    Id = order.Id,
                    UpdatedOnUtc = DateTime.UtcNow,
                    Items = order.Items.ToDictionary(i=>i.FoodId, i=>i.Quantity),
                    Source = "CancelOrder",
                    OrderStatus = order.Status
                },
                cancellationToken);
            
            var refund = new TopUpRequest
            {
                CustomerId = request.CustomerId,
                Amount = order.TotalAmount,
                SourceId = $"{order.OrderNumber} refund!"
            };

            await _accountService.TopUpBalanceAsync(refund);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new Exception("Order cannot be cancelled!");
        }

        return new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            OrderDate = order.CreatedAt,
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