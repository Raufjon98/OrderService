using MessagePack;

namespace OrderService.Contracts.OrderItem.Responses;

[MessagePackObject]
public record OrderItemResponse
{
    [Key(0)]
    public required Guid OrderId { get; set; }
    [Key(1)]
    public required string FoodId { get; set; }
    [Key(2)]
    public int Quantity { get; set; }
    [Key(3)]
    public decimal UnitPrice { get; set; }

    [Key(4)]
    public decimal Subtotal => Quantity * UnitPrice;
}