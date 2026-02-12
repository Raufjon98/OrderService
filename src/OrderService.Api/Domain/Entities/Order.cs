using OrderService.Contracts.Enums;

namespace OrderService.Api.Domain.Entities;

public class Order : BaseAuditableEntity
{
    public required Guid OrderNumber { get; set; } 
    public required Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public decimal TotalAmount => Items.Sum(i => i.Subtotal);
    public Delivery? Delivery { get; set; }
}