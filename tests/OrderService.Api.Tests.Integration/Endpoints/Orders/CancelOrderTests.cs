using CatalogService.Contracts.Food.Responses;
using FluentAssertions;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using NSubstitute;
using OrderService.Api.Tests.Integration.Models;
using OrderService.Contracts.Cart.Requests;
using OrderService.Contracts.CartItem.Requests;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Interfaces;
using PaymentService.Contracts.Account.Responses;

namespace OrderService.Api.Tests.Integration.Endpoints.Orders;

public class CancelOrderTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    
    public CancelOrderTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _orderService = MagicOnionClient.Create<IOrderService>(channel);
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }
    
    [Fact]
    public async Task CancelOrder_CancelsOrder_WhenOrderExists()
    {
        //Arrange
        var scenario = new OrderTestScenarioBuilder(
            _factory,
            _cartService,
            _orderService);
        var userId = scenario.UserId;
        var order = await scenario.CreateOrderAsync();

        //Act
        var response = await _orderService.CancelOrderAsync(userId, order.Id);
        
        //Assert
        response.Should().NotBeNull();
        response.CustomerId.Should().Be(userId);
        response.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task CancelOrder_ThrowsException_WhenOrderReady()
    {
        //Arrange
        var scenario = new OrderTestScenarioBuilder(
            _factory,
            _cartService,
            _orderService);
        var userId = scenario.UserId;
        var order = await scenario.CreateOrderAsync();

        await _orderService.StartPreparationAsync(userId, order.Id);
        await _orderService.MarkAsReadyAsync(userId, order.Id);
        
        //Act
        Func<Task> act = async () => await _orderService.CancelOrderAsync(userId, order.Id);
        
        //Assert
        await act.Should().ThrowAsync<RpcException>("Order cannot be cancelled!");

    }
}