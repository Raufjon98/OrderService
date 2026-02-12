using MessagePack;
using OrderService.Contracts.OrderItem.Requests;

namespace OrderService.Contracts.Order.Requests;

[MessagePackObject]
public record AddToOrderRequest
{
    [Key(0)]
    public Guid OrderId { get; set; }
    [Key(1)]
    public List<OrderItemRequest> Items { get; set; } =  new List<OrderItemRequest>();
}