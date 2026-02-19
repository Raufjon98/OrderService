using FluentAssertions;
using Grpc.Core;
using MagicOnion.Client;
using OrderService.Api.Tests.Integration.Models;
using OrderService.Contracts.Interfaces;

namespace OrderService.Api.Tests.Integration.Endpoints.Orders;

public class GetOrderStatusTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;

    public GetOrderStatusTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _orderService = MagicOnionClient.Create<IOrderService>(channel);
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }

    [Fact]
    public async Task GetOrderStatus_ReturnsOrderStatus_WhenOrderExists()
    {
        //Arrange
        var scenario = new OrderTestScenarioBuilder(
            _factory,
            _cartService,
            _orderService);
        var order = await scenario.CreateOrderAsync();
        var userId = scenario.UserId;

        //Act
        var response = await _orderService.GetOrderStatusAsync(userId, order.Id);

        //Assert
        response.Should().Be(order.Status);
    }

    [Fact]
    public async Task GetOrderStatus_ReturnsZero_WhenOrderDoesNotExists()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        //Act
        Func<Task> act = async () => await _orderService.GetOrderStatusAsync(userId, orderId);

        //Assert
        await act.Should().ThrowAsync<RpcException>($"Entity Order with key {orderId} not found!");
    }
}