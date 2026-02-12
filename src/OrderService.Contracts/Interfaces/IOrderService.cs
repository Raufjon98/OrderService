using MagicOnion;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Order.Requests;
using OrderService.Contracts.Order.Responses;

namespace OrderService.Contracts.Interfaces;

public interface IOrderService : IService<IOrderService>
{
    UnaryResult<OrderResponse> CreateOrderAsync(Guid customerId);
    UnaryResult<OrderResponse> CancelOrderAsync(Guid customerId, Guid orderId);
    UnaryResult<OrderResponse> CompleteOrderAsync(Guid customerId, Guid orderId);
    UnaryResult<OrderResponse> MarkAsReadyAsync(Guid customerId, Guid orderId);
    UnaryResult<OrderResponse> StartPreparationAsync(Guid customerId, Guid orderId);
    UnaryResult<OrderResponse> GetOrderAsync(Guid customerId, Guid orderId);

    UnaryResult<List<OrderResponse>> GetOrdersAsync(Guid customerId, int page = 1, int pageSize = 10,
        OrderStatus? orderStatus = null, DateTime? dateFrom = null, DateTime? dateTo = null);

    UnaryResult<OrderStatus> GetOrderStatusAsync(Guid customerId, Guid orderId);
    UnaryResult<OrderResponse> AddToOrderAsync(Guid customerId, AddToOrderRequest request);
    UnaryResult<OrderResponse> RemoveFromOrderAsync(Guid customerId, RemoveFromOrderRequest request);
}