using MagicOnion;
using MagicOnion.Server;
using MediatR;
using OrderService.Api.Features.Carts.Commands.AddItemToCart;
using OrderService.Api.Features.Carts.Commands.CreateCart;
using OrderService.Api.Features.Carts.Commands.RemoveItemFromCart;
using OrderService.Api.Features.Carts.Queries.GetCart;
using OrderService.Contracts.Cart.Requests;
using OrderService.Contracts.Cart.Responses;
using OrderService.Contracts.Interfaces;


namespace OrderService.Api.MagicOnion.Services;

public class CartService : ServiceBase<ICartService>, ICartService
{
    private readonly IMediator _mediator;

    public CartService(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async UnaryResult<CartResponse> CreateCartAsync(Guid customerId, CreateCartRequest request)
    {
        var command = new CreateCartCommand(customerId, request);
        var result = await _mediator.Send(command);
        return result;
    }

    public async UnaryResult<CartResponse> GetCartAsync(Guid customerId)
    {
        var query = new GetCartQuery(customerId);
        var result = await _mediator.Send(query);
        return result;
    }

    public async UnaryResult<CartResponse> AddItemToCartAsync(Guid customerId, CartItemsRequest request)
    {
        var command = new AddItemToCartCommand(customerId, request);
        var result = await _mediator.Send(command);
        return result;
    }

    public async UnaryResult<CartResponse> RemoveItemFromCartAsync(Guid customerId, CartItemsRequest request)
    {
        var command =new RemoveItemFromCartCommand(customerId, request);
        var result = await _mediator.Send(command);
        return result;
    }
}