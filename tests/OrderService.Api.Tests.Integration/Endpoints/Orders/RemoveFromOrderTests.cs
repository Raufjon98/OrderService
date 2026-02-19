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
using OrderService.Contracts.Order.Requests;
using OrderService.Contracts.OrderItem.Requests;
using PaymentService.Contracts.Account.Responses;

namespace OrderService.Api.Tests.Integration.Endpoints.Orders;

public class RemoveFromOrderTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;

    public RemoveFromOrderTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _orderService = MagicOnionClient.Create<IOrderService>(channel);
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }

    [Fact]
    public async Task RemoveFromOrder_RemovesItem_WhenItemExists()
    {
        //Arrange
        var scenario = new OrderTestScenarioBuilder(
            _factory,
            _cartService,
            _orderService);
        var userId = scenario.UserId;
        var order = await scenario.CreateOrderAsync();

        var removeItemFromOrderRequest = new RemoveFromOrderRequest
        {
            OrderId = order.Id,
            Items =
            {
                new OrderItemRequest
                {
                    FoodId = order.Items[0].FoodId,
                    Quantity = 2
                }
            }
        };
        
        //Act
        var response = await _orderService.RemoveFromOrderAsync(userId, removeItemFromOrderRequest);
        
        //Assert
        response.Should().NotBeNull();
        response.CustomerId.Should().Be(userId);
        response.Items.Should().Contain(i => i.FoodId == order.Items[0].FoodId && i.Quantity == 1);
    }

    [Fact]
    public async Task RemoveFromOrder_ThrowsException_WhenItemDoesNotExist()
    {
        //Arrange
        var scenario = new OrderTestScenarioBuilder(
            _factory,
            _cartService,
            _orderService);
        var userId = scenario.UserId;
        var order = await scenario.CreateOrderAsync();
        var inValidFoodId = Guid.NewGuid().ToString(); 
        
        var removeItemFromOrderRequest = new RemoveFromOrderRequest
        {
            OrderId = order.Id,
            Items =
            {
                new OrderItemRequest
                {
                    FoodId = inValidFoodId,
                    Quantity = 2
                }
            }
        };
        
        //Act
        Func<Task> act= async () => await _orderService.RemoveFromOrderAsync(userId, removeItemFromOrderRequest);
        
        //Assert
        await act.Should().ThrowAsync<RpcException>($"Order doesn't contains item with key:{inValidFoodId}");
    }
}