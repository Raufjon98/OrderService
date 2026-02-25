using MagicOnion;
using MagicOnion.Server;
using MediatR;
using OrderService.Api.Features.Orders.Commands.AddToOrder;
using OrderService.Api.Features.Orders.Commands.CancelOrder;
using OrderService.Api.Features.Orders.Commands.CompleteOrder;
using OrderService.Api.Features.Orders.Commands.CreateOrder;
using OrderService.Api.Features.Orders.Commands.MarkAsReady;
using OrderService.Api.Features.Orders.Commands.RemoveFromOrder;
using OrderService.Api.Features.Orders.Commands.StartPreparation;
using OrderService.Api.Features.Orders.Queries.GetOrder;
using OrderService.Api.Features.Orders.Queries.GetOrders;
using OrderService.Api.Features.Orders.Queries.GetOrderStatus;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Interfaces;
using OrderService.Contracts.Order.Requests;
using OrderService.Contracts.Order.Responses;

namespace OrderService.Api.MagicOnion.Services;

public class OrderService : ServiceBase<IOrderService>, IOrderService
{
    private readonly IMediator _mediator;

    public OrderService(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async UnaryResult<OrderResponse> CreateOrderAsync(Guid customerId)
    {
        var command = new CreateOrderCommand(customerId);
        var result = await _mediator.Send(command);
        return result;
    }

    public async UnaryResult<OrderResponse> CancelOrderAsync(Guid customerId, Guid orderId)
    {
        var command = new CancelOrderCommand(customerId, orderId);
        var result = await _mediator.Send(command);
        return result;
    }

    public async UnaryResult<OrderResponse> CompleteOrderAsync(Guid customerId ,Guid orderId)
    {
        var command = new CompleteOrderCommand(orderId);
        var result = await _mediator.Send(command);
        return result;
    }

    public async UnaryResult<OrderResponse> MarkAsReadyAsync(Guid customerId, Guid orderId)
    {
        var command = new MarkAsReadyCommand(orderId);
        var result = await _mediator.Send(command);
        return result;
    }

    public async UnaryResult<OrderResponse> StartPreparationAsync(Guid customerId, Guid orderId)
    {
        var command = new StartPreparationCommand(orderId);
        var result = await _mediator.Send(command);
        return result;
    }

    public async UnaryResult<OrderResponse> GetOrderAsync(Guid customerId, Guid orderId)
    {
        var query = new GetOrderQuery(customerId, orderId);
        var result = await _mediator.Send(query);
        return result;
    }


    public async UnaryResult<List<OrderResponse>> GetOrdersAsync(
        Guid customerId, 
        int page = 1,
        int pageSize = 10,
        OrderStatus? orderStatus = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var query = new GetOrdersQuery(customerId, page, pageSize, orderStatus, dateFrom, dateTo);
        var result = await _mediator.Send(query);
        return result;
    }

    public async UnaryResult<OrderStatus> GetOrderStatusAsync(Guid customerId, Guid orderId)
    {
        var query = new GetOrderStatusQuery(customerId, orderId);
        var result = await _mediator.Send(query);
        return result;
    }

    public async UnaryResult<OrderResponse> AddToOrderAsync(Guid customerId, AddToOrderRequest request)
    {
        var command = new AddToOrderCommand(customerId, request);
        var result = await _mediator.Send(command);
        return result;
    }

    public async UnaryResult<OrderResponse> RemoveFromOrderAsync(Guid customerId, RemoveFromOrderRequest request)
    {
        var command = new RemoveFromOrderCommand(customerId, request);
        var result = await _mediator.Send(command);
        return result;
    }
}