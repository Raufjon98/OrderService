using MessagePack;

namespace OrderService.Contracts.OrderItem.Requests;

[MessagePackObject]
public record OrderItemRequest
{
    [Key(0)]
    public required string FoodId { get; set; }
    [Key(1)]
    public int Quantity { get; set; }
}