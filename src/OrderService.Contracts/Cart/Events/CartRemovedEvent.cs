namespace OrderService.Contracts.Cart.Events;

public record CartRemovedEvent
{
    public Guid Id { get; init; }
    public DateTime RemovedOnUtc { get; set; }
}