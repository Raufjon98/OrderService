using CatalogService.Contracts.Food.Responses;
using FluentAssertions;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using NSubstitute;
using OrderService.Api.Tests.Integration.Models;
using OrderService.Contracts.Cart.Requests;
using OrderService.Contracts.CartItem.Requests;
using OrderService.Contracts.Interfaces;

namespace OrderService.Api.Tests.Integration.Endpoints.Carts;

public class RemoveItemFromCartTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly  ICartService _cartService;
    
    public RemoveItemFromCartTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }

    [Fact]
    public async Task RemoveItemFromCart_RemovesItems_WhenDataValid()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid().ToString();

        var food = TestDataFactory.CreateFoodResponse();
        food.RestaurantId = restaurantId;
        var foodSecond = TestDataFactory.CreateFoodResponse();
        foodSecond.RestaurantId = restaurantId;

        _factory.FoodServiceMock.GetFoodAsync(food.Id!).Returns(new UnaryResult<FoodResponse>(food));
        _factory.FoodServiceMock.GetFoodAsync(foodSecond.Id!).Returns(new UnaryResult<FoodResponse>(foodSecond));
       
        var itemsToAdd = new CartItemsRequest
        {
            Items =
            {
                new CartItemRequest
                {
                    FoodId = food.Id!,
                    Quantity = 3,
                },
                new CartItemRequest
                {
                    FoodId = foodSecond.Id!,
                    Quantity = 5,
                }
            }
        };
        
        var itemsForRemove = new CartItemsRequest
        {
            Items =
            {
                new CartItemRequest
                {
                    FoodId = foodSecond.Id!,
                    Quantity = 3,
                }
            }
        };
        
        var addItemResponse = await _cartService.AddItemToCartAsync(userId, itemsToAdd);
        addItemResponse.CustomerId.Should().Be(userId);
        addItemResponse.Items.Should().Contain(c=>c.FoodId == food.Id);
        addItemResponse.Items.Should().Contain(c=>c.FoodId == foodSecond.Id);
        
        //Act
        var response = await _cartService.RemoveItemFromCartAsync(userId, itemsForRemove);
        
        //Assert
        response.CustomerId.Should().Be(userId);
        response.Items.Should().Contain(c=>c.FoodId == foodSecond.Id && c.Quantity == 2);
        response.Items.Should().Contain(c=>c.FoodId == food.Id && c.Quantity == 3);
    }


    [Fact]
    public async Task RemoveItemFromCart_DoesNotRemove_WhenDataInvalid()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var invalidFoodId = Guid.NewGuid().ToString();
        
        var food = TestDataFactory.CreateFoodResponse();
        
        _factory.FoodServiceMock.GetFoodAsync(food.Id!).Returns(new UnaryResult<FoodResponse>(food));

        var itemsToAdd = new CartItemsRequest
        {
            Items =
            {
                new CartItemRequest
                {
                    FoodId = food.Id!,
                    Quantity = 3,
                }
            }
        };
        
        var invalidItemForRemove = new CartItemsRequest
        {
            Items =
            {
                new CartItemRequest
                {
                    FoodId = invalidFoodId,
                    Quantity = 3,
                }
            }
        };
        
        var addItemResponse = await _cartService.AddItemToCartAsync(userId, itemsToAdd);
        addItemResponse.CustomerId.Should().Be(userId);
        addItemResponse.Items.Should().Contain(c=>c.FoodId == food.Id && c.Quantity == 3);

        //Act
        Func<Task> act= async () => await _cartService.RemoveItemFromCartAsync(userId, invalidItemForRemove);
        
        //Assert
        await act.Should().ThrowAsync<RpcException>($"You do not have item {invalidFoodId} for this cart!");

    }
}