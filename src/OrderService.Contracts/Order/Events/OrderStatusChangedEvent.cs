using OrderService.Contracts.Enums;

namespace OrderService.Contracts.Order.Events;

public record OrderStatusChangedEvent
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderStatusChangedOnUtc { get; set; }
}