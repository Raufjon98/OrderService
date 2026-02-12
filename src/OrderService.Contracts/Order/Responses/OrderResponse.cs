using System.Security.Principal;
using MessagePack;
using OrderService.Contracts.Enums;
using OrderService.Contracts.OrderItem.Responses;

namespace OrderService.Contracts.Order.Responses;

[MessagePackObject]
public record OrderResponse
{
    [Key(0)]
    public required Guid OrderNumber { get; set; }
    [Key(1)]
    public required Guid CustomerId { get; set; }
    [Key(2)]
    public DateTime OrderDate { get; set; }
    [Key(3)]
    public OrderStatus Status { get; set; }
    [Key(4)]
    public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();

    [Key(5)]
    public decimal TotalAmount => Items.Sum(i => i.Subtotal);

    [Key(6)]
    public Guid Id { get; set; }

}