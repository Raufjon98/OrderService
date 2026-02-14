namespace OrderService.Contracts.Cart.Events;

public record CartUpdatedEvent
{
    public Guid CustomerId { get; init; }
    public DateTime UpdatedOnUtc { get; init; }
    public List<string> FoodIds { get; set; } = new List<string>();
    public required string Source { get; init; } 
}