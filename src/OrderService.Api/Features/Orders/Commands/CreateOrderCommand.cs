using CatalogService.Contracts.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Cart.Events;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Order.Events;
using OrderService.Contracts.Order.Responses;
using OrderService.Contracts.OrderItem.Responses;
using PaymentService.Contracts.Account.Requests;
using PaymentService.Contracts.Interfaces;


namespace OrderService.Api.Features.Orders.Commands;

public record CreateOrderCommand(Guid CustomerId) : IRequest<OrderResponse>;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly OrderDbContext _context;
    private readonly IAccountService _accountService;
    private readonly IFoodService _foodService;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderHandler(OrderDbContext context,
        IAccountService accountService,
        IFoodService foodService, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _accountService = accountService;
        _foodService = foodService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (cart is null)
        {
            throw new Exception("Your cart is empty!");
        }

        var orderId = Guid.NewGuid();
        string? orderRestaurantId = null;

        foreach (var item in cart.Items)
        {
            var food = await _foodService.GetFoodAsync(item.FoodId);
            var restaurantId = food.RestaurantId;

            if (orderRestaurantId == null)
            {
                orderRestaurantId = restaurantId;
            }
            else if (orderRestaurantId != restaurantId)
            {
                throw new Exception($"You couldn't add food from several restaurants!");
            }
        }

        Order order = new Order()
        {
            Id = orderId,
            OrderNumber = Guid.NewGuid(),
            CustomerId = cart.CustomerId,
            Status = OrderStatus.PendingPayment,
            Items = cart.Items.Select(i => new OrderItem
                {
                    OrderId = orderId,
                    FoodId = i.FoodId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                }
            ).ToList()
        };

        await _context.Orders.AddAsync(order, cancellationToken);
        cart.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);

        await _accountService.WithdrawBalanceAsync(new WithdrawRequest
        {
            SourceId = orderId.ToString(),
            CustomerId = cart.CustomerId,
            Amount = order.TotalAmount
        });

        await _publishEndpoint.Publish(
            new CartRemovedEvent()
            {
                CustomerId = request.CustomerId,
                RemovedOnUtc = DateTime.UtcNow
            },
            cancellationToken);

        await _publishEndpoint.Publish(
            new OrderCreatedEvent
            {
                CustomerId = cart.CustomerId,
                Id = orderId,
                CreatedOnUtc = DateTime.UtcNow,
            },
            cancellationToken);

        return new OrderResponse()
        {
            Id = orderId,
            OrderNumber = order.OrderNumber,
            OrderDate = order.CreatedAt,
            CustomerId = cart.CustomerId,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                OrderId = i.OrderId,
                FoodId = i.FoodId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}