namespace OrderService.Contracts.Cart.Events;

public record CartRemovedEvent
{
    public Guid CustomerId { get; init; }
    public DateTime RemovedOnUtc { get; set; }
}