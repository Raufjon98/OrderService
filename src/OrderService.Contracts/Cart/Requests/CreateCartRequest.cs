using MessagePack;
using OrderService.Contracts.CartItem.Requests;

namespace OrderService.Contracts.Cart.Requests;

[MessagePackObject]
public record CreateCartRequest
{
    [Key(0)]
    public required CartItemRequest[] Items { get; set; }
}