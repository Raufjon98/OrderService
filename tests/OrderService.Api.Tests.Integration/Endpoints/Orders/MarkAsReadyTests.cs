using FluentAssertions;
using Grpc.Core;
using MagicOnion.Client;
using OrderService.Api.Tests.Integration.Models;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Interfaces;
using OrderService.Contracts.Order.Responses;

namespace OrderService.Api.Tests.Integration.Endpoints.Orders;

public class MarkAsReadyTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;

    public MarkAsReadyTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _orderService = MagicOnionClient.Create<IOrderService>(channel);
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }

    [Fact]
    public async Task MarkAsReady_MarkAsReady_WhenOrderPreparing()
    {
        //Arrange
        var scenario = new OrderTestScenarioBuilder(
            _factory,
            _cartService,
            _orderService);
        var userId = scenario.UserId;
        var order = await scenario.CreateOrderAsync();
        await _orderService.StartPreparationAsync(userId, order.Id);
        
        //Act
        var response = await _orderService.MarkAsReadyAsync(userId, order.Id);
        
        //Assert
        response.Should().BeOfType<OrderResponse>();
        response.CustomerId.Should().Be(order.CustomerId);
        response.Status.Should().Be(OrderStatus.Ready);
    }

    [Fact]
    public async Task MarkAsReady_ThrowsException_WhenOrderIsNotPreparing()
    {
        //Arrange
        var scenario = new OrderTestScenarioBuilder(
            _factory,
            _cartService,
            _orderService);
        var userId = scenario.UserId;
        var order = await scenario.CreateOrderAsync();
        await _orderService.StartPreparationAsync(userId, order.Id);
        
        //Act
        Func<Task> act = async () => await _orderService.MarkAsReadyAsync(userId, order.Id);
        
        //Arrange 
        await act.Should().ThrowAsync<RpcException>("Only preparing orders can be support!");
    }
}