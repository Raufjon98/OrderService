using MessagePack;

namespace OrderService.Contracts.CartItem.Responses;

[MessagePackObject]
public record CartItemResponse
{
    [Key(0)]
    public required Guid CartId { get; set; }
    [Key(1)]
    public required string FoodId { get; set; }
    [Key(2)]
    public int Quantity { get; set; }
    [Key(3)]
    public decimal UnitPrice { get; set; }

    [Key(4)]
    public decimal Subtotal => Quantity * UnitPrice;
}