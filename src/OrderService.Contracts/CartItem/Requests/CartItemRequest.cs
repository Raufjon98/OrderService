using MessagePack;

namespace OrderService.Contracts.CartItem.Requests;

[MessagePackObject]
public record CartItemRequest
{
    [Key(0)]
    public required string FoodId { get; set; }
    [Key(1)]
    public int Quantity { get; set; }
}