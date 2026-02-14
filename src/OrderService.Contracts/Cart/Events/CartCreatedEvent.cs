namespace OrderService.Contracts.Cart.Events;

public record CartCreatedEvent
{
    public Guid CustomerId { get; init; }
    public Guid[] ItemsIds { get; init; } = [];
    public DateTime CreatedOnUtc { get; init; }
}