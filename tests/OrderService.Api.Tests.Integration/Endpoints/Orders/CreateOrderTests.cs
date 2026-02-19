using CatalogService.Contracts.Food.Responses;
using FluentAssertions;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using NSubstitute;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Tests.Integration.Models;
using OrderService.Contracts.Cart.Requests;
using OrderService.Contracts.CartItem.Requests;
using OrderService.Contracts.Interfaces;
using OrderService.Contracts.Order.Responses;
using PaymentService.Contracts.Account.Responses;

namespace OrderService.Api.Tests.Integration.Endpoints.Orders;

public class CreateOrderTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;

    public CreateOrderTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _orderService = MagicOnionClient.Create<IOrderService>(channel);
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }

    [Fact]
    public async Task CreateOrder_CreatesOrder_WhenCartExists()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var food = TestDataFactory.CreateFoodResponse();
        _factory.FoodServiceMock.GetFoodAsync(food.Id!).Returns(new UnaryResult<FoodResponse>(food));

        var account = new AccountResponse
        {
            CustomerId = userId,
            Balance = 100,

        };
        
        _factory.AccountServiceMock.CreateAccountAsync(userId).Returns(new UnaryResult<AccountResponse>(account));
        
        var cartResponse = await _cartService.CreateCartAsync(userId, new CreateCartRequest
        {
            Items =
            [
                new CartItemRequest
                {
                    FoodId = food.Id!,
                    Quantity = 1
                }
            ]
        });
        

        //Act
        var response = await _orderService.CreateOrderAsync(userId);
        
        //Assert
        response.Should().NotBeNull();
        response.CustomerId.Should().Be(userId);
        response.Should().BeOfType<OrderResponse>();
        response.Items.Should().Contain(i => i.FoodId == food.Id!);
    }

    [Fact]
    public async Task CreateOrder_ThrowsException_WhenCartDoesNotExist()
    {
        //Act
        Func<Task> act = async() =>  await _orderService.CreateOrderAsync(Guid.NewGuid());
        
        //Assert
        await act.Should().ThrowAsync<RpcException>("Your cart is empty!");
    }
}