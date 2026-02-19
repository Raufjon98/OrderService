using CatalogService.Contracts.Food.Responses;
using FluentAssertions;
using MagicOnion;
using MagicOnion.Client;
using NSubstitute;
using OrderService.Api.Tests.Integration.Models;
using OrderService.Contracts.Cart.Requests;
using OrderService.Contracts.Cart.Responses;
using OrderService.Contracts.CartItem.Requests;
using OrderService.Contracts.Interfaces;

namespace OrderService.Api.Tests.Integration.Endpoints.Carts;

public class AddItemToCartTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly ICartService _cartService;

    public AddItemToCartTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }

    [Fact]
    public async Task AddItemToCart_CreatesCart_WhenDataValidAndCartEmpty()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid().ToString();
        var food = TestDataFactory.CreateFoodResponse();
        var foodSecond = TestDataFactory.CreateFoodResponse();
        foodSecond.RestaurantId = restaurantId;
        food.RestaurantId = restaurantId;
        
        _factory.FoodServiceMock.GetFoodAsync(food.Id!).Returns(new UnaryResult<FoodResponse>(food));
        _factory.FoodServiceMock.GetFoodAsync(foodSecond.Id!).Returns(new UnaryResult<FoodResponse>(foodSecond));

        var addItems = new CartItemsRequest
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

        //Act
        var response = await _cartService.AddItemToCartAsync(userId, addItems);

        //Assert
        response.Should().NotBeNull();
        response.Should().BeOfType<CartResponse>();
        response.CustomerId.Should().Be(userId);
        response.Items.Should().Contain(i => i.FoodId == food.Id);
        response.Items.Should().Contain(i => i.FoodId == foodSecond.Id);
    }

    [Fact]
    public async Task AddItemToCart_ThrowsException_WhenFoodRestaurantsSeveral()
    {
        //Arrange
        var userId = Guid.NewGuid();

        var food = TestDataFactory.CreateFoodResponse();
        var foodSecond = TestDataFactory.CreateFoodResponse();

        _factory.FoodServiceMock.GetFoodAsync(food.Id!).Returns(new UnaryResult<FoodResponse>(food));
        _factory.FoodServiceMock.GetFoodAsync(foodSecond.Id!).Returns(new UnaryResult<FoodResponse>(foodSecond));

        var addItems = new CartItemsRequest
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

        //Act
        Func<Task> act = async () => await _cartService.AddItemToCartAsync(userId, addItems);

        //Assert
       await act.Should().ThrowAsync<Exception>($"You couldn't add food from several restaurants!");
    }
}