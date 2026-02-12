using MessagePack;
using OrderService.Contracts.CartItem.Responses;

namespace OrderService.Contracts.Cart.Responses;

[MessagePackObject]
public record CartResponse
{
    [Key(0)] 
    public Guid Id { get; set; }
    [Key(1)] 
    public Guid CustomerId { get; set; }
    [Key(2)] 
    public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();
    [Key(3)] 
    public decimal TotalAmount => Items.Sum(i => i.Subtotal);
}