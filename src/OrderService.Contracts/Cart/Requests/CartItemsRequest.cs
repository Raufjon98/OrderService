using MessagePack;
using OrderService.Contracts.CartItem.Requests;

namespace OrderService.Contracts.Cart.Requests;
[MessagePackObject]
public record CartItemsRequest
{
    [Key(0)]
    public List<CartItemRequest> Items { get; set; } = new List<CartItemRequest>();
};