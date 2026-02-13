namespace OrderService.Contracts.Order.Events;

public record OrderCreatedEvent
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public List<Guid> ItemsIds { get; init; } = new List<Guid>();
    public DateTime CreatedOnUtc { get; init; }
}