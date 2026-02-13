using OrderService.Contracts.Enums;

namespace OrderService.Contracts.Order.Events;

public record OrderUpdatedEvent
{
    public Guid Id { get; init; }
    public DateTime UpdatedOnUtc { get; init; }
    public Dictionary<string, int> Items { get; set; } = new();
    public required string Source { get; init; }
    public OrderStatus OrderStatus { get; set; }
}