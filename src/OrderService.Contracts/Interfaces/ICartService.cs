using MagicOnion;
using OrderService.Contracts.Cart.Requests;
using OrderService.Contracts.Cart.Responses;
using OrderService.Contracts.CartItem.Requests;
using OrderService.Contracts.CartItem.Responses;

namespace OrderService.Contracts.Interfaces;

public interface ICartService : IService<ICartService>
{
    UnaryResult<CartResponse> CreateCartAsync(Guid customerId, CreateCartRequest request);
    UnaryResult<CartResponse> GetCartAsync(Guid customerId);
    UnaryResult<CartResponse> AddItemToCartAsync(Guid customerId, CartItemsRequest request);
    UnaryResult<CartResponse> RemoveItemFromCartAsync(Guid customerId, CartItemsRequest request);
}