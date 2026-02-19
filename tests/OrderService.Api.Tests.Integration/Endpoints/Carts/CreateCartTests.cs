using System.ComponentModel.DataAnnotations;
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

public class CreateCartTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly ICartService _cartService;

    public CreateCartTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }

    [Fact]
    public async Task CreateCart_CreatesCart_WhenDataValid()
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

        //Act
        var response = await _cartService.CreateCartAsync(userId, createCartRequest);

        //Assert
        response.Should().BeOfType<CartResponse>();
        response.CustomerId.Should().Be(userId);
        response.Items.Should().Contain(i => i.FoodId == food.Id);
    }

    [Fact]
    public async Task CreateCart_ReturnsError_WhenDataInvalid()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var invalidFoodId = Guid.NewGuid().ToString();
        var food = TestDataFactory.CreateFoodResponse();
    
        _factory.FoodServiceMock.GetFoodAsync(food.Id!)
            .Returns(new UnaryResult<FoodResponse>(food));

        var createCartRequest = new CreateCartRequest
        {
            Items =
            [
                new CartItemRequest
                {
                    FoodId =invalidFoodId,
                    Quantity = 1
                }
            ]
        };

        //Act
        Func<Task> act = async () => await _cartService.CreateCartAsync(userId, createCartRequest);

        //Assert
        await act.Should().ThrowAsync<RpcException>($"Entity {"Food"} with key {invalidFoodId} not found!");
    }
}