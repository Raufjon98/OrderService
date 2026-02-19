using CatalogService.Contracts.Food.Responses;
using FluentAssertions;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using NSubstitute;
using OrderService.Api.Tests.Integration.Models;
using OrderService.Contracts.Cart.Requests;
using OrderService.Contracts.Cart.Responses;
using OrderService.Contracts.CartItem.Requests;
using OrderService.Contracts.Interfaces;

namespace OrderService.Api.Tests.Integration.Endpoints.Carts;

public class GetCartTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly ICartService _cartService;

    public GetCartTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }

    [Fact]
    public async Task GetCart_ReturnsCartResponse_WhenCartExists()
    {
        //Arrange
        var userId = Guid.NewGuid();

        var food = TestDataFactory.CreateFoodResponse();

        _factory.FoodServiceMock.GetFoodAsync(food.Id!)
            .Returns(new UnaryResult<FoodResponse>(food));

        var createCartRequest = new CreateCartRequest
        {
            Items =
            [
                new CartItemRequest
                {
                    FoodId = food.Id!,
                    Quantity = 1
                }
            ]
        };
        var createCartResponse = await _cartService.CreateCartAsync(userId, createCartRequest);
        createCartResponse.CustomerId.Should().Be(userId);

        //Act
        var response = await _cartService.GetCartAsync(userId);

        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<CartResponse>();
        response.Should().BeEquivalentTo(createCartResponse);
    }

    [Fact]
    public async Task GetCart_ThrowsException_WhenCartDoesNotExist()
    {
        //Arrange
        var userId = Guid.NewGuid();
        
        //Act
        Func<Task> act = async () => await _cartService.GetCartAsync(userId);
        
        //Assert
        await act.Should().ThrowAsync<RpcException>($"Entity {"Cart"} with key {userId} not found!");
    }
}