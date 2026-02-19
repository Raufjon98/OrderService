using FluentAssertions;
using Grpc.Core;
using MagicOnion.Client;
using OrderService.Api.Tests.Integration.Models;
using OrderService.Contracts.Interfaces;

namespace OrderService.Api.Tests.Integration.Endpoints.Orders;

public class GetOrderTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;


    public GetOrderTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _orderService = MagicOnionClient.Create<IOrderService>(channel);
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }

    [Fact]
    public async Task GetOrder_ReturnsOrder_WhenOrderExists()
    {
        //Arrange
        var scenario = new OrderTestScenarioBuilder(
            _factory,
            _cartService,
            _orderService);
        var userId = scenario.UserId;
        var order = await scenario.CreateOrderAsync();
        
        //Act
        var response = await _orderService.GetOrderAsync(userId, order.Id);
        
        //Assert
        response.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task GetOrder_ThrowsException_WhenOrderDoesNotExist()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        
        //Act
        Func<Task> act = async () => await _orderService.GetOrderAsync(userId, orderId);
        
        //Assert
        await act.Should().ThrowAsync<RpcException>($"Entity Order with key {orderId} not found!");
    }
    
}