using FluentAssertions;
using MagicOnion.Client;
using OrderService.Api.Tests.Integration.Models;
using OrderService.Contracts.Interfaces;
using OrderService.Contracts.Order.Responses;

namespace OrderService.Api.Tests.Integration.Endpoints.Orders;

public class GetOrdersTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;

    public GetOrdersTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _orderService = MagicOnionClient.Create<IOrderService>(channel);
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }

    [Fact]
    public async Task GetOrders_ReturnsAllOrders()
    {
        //Arrange
        var scenario = new OrderTestScenarioBuilder(
            _factory,
            _cartService,
            _orderService);
        var order = await scenario.CreateOrderAsync();
        var userId = scenario.UserId;
        
        //Act
        var response = await _orderService.GetOrdersAsync(userId);
        
        //Assert
        response.Should().BeOfType<List<OrderResponse>>();
        response.Should().Contain(r=>r.Id == order.Id && r.CustomerId == userId && r.Status == order.Status);
    }

    [Fact]
    public async Task GetOrders_ReturnsEmptyOrders()
    {
        //Arrange
        var userId = Guid.NewGuid();
        
        //Act
        var response = await _orderService.GetOrdersAsync(userId);
        
        //Assert
        response.Should().BeOfType<List<OrderResponse>>();
        response.Should().BeEmpty();
    }
}