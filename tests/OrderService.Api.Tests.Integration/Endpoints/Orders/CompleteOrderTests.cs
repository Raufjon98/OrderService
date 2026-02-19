using FluentAssertions;
using Grpc.Core;
using MagicOnion.Client;
using OrderService.Api.Tests.Integration.Models;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Interfaces;

namespace OrderService.Api.Tests.Integration.Endpoints.Orders;

public class CompleteOrderTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;

    public CompleteOrderTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _orderService = MagicOnionClient.Create<IOrderService>(channel);
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }

    [Fact]
    public async Task CompleteOrder_CompletesOrder_WhenOrderIsReady()
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
        var response = await _orderService.CompleteOrderAsync(userId, order.Id);

        //Assert
        response.Should().NotBeNull();
        response.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task CompleteOrder_ThrowsException_WhenOrderIsNotReady()
    {
        //Arrange
        var scenario = new OrderTestScenarioBuilder(
            _factory,
            _cartService,
            _orderService);
        var userId = scenario.UserId;
        var order = await scenario.CreateOrderAsync();
        
        //Act
        Func<Task> act = async () => await _orderService.CompleteOrderAsync(userId, order.Id);
        
        //Assert
        await act.Should().ThrowAsync<RpcException>("Only Ready orders are supported!");
    }
}