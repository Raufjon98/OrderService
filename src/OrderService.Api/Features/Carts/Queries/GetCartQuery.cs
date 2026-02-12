using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Features.Common.Exceptions;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Cart.Responses;
using OrderService.Contracts.CartItem.Responses;

namespace OrderService.Api.Features.Carts.Queries;

public record GetCartQuery(Guid CustomerId) : IRequest<CartResponse>;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartResponse>
{
    private readonly OrderDbContext _context;

    public GetCartQueryHandler(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<CartResponse> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Where(c => c.CustomerId == request.CustomerId && c.IsDeleted == false)
            .Select(c => new CartResponse
        {
            Id = c.Id,
            CustomerId = c.CustomerId,
            Items = c.Items.Select(i => new CartItemResponse
            {
                CartId = c.Id,
                UnitPrice = i.UnitPrice,
                FoodId = i.FoodId,
                Quantity = i.Quantity,
            }).ToList()
            
        }).FirstOrDefaultAsync(cancellationToken);
        if (cart == null)
        {
            throw new NotFoundException(nameof(Cart), request.CustomerId.ToString());
        }

        return cart;
    }
}