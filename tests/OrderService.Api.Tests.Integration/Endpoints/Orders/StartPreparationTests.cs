using FluentAssertions;
using Grpc.Core;
using MagicOnion.Client;
using OrderService.Api.Tests.Integration.Models;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Interfaces;
using OrderService.Contracts.Order.Responses;

namespace OrderService.Api.Tests.Integration.Endpoints.Orders;

public class StartPreparationTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    

    public StartPreparationTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _orderService = MagicOnionClient.Create<IOrderService>(channel);
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }


    [Fact]
    public async Task StartPreparation_UpdatesStatus_WhenOrderPendingOrConfirmed()
    {
        //Arrange
        var scenario = new OrderTestScenarioBuilder(
            _factory,
            _cartService,
            _orderService);
        var userId = scenario.UserId;
        var order = await scenario.CreateOrderAsync();
        
        //Act
        var response = await _orderService.StartPreparationAsync(userId, order.Id);
        
        //Assert
        response.Should().BeOfType<OrderResponse>();
        response.CustomerId.Should().Be(order.CustomerId);
        response.Status.Should().Be(OrderStatus.Preparing);
    }
    
    [Fact]
    public async Task StartPreparation_ThrowsException_WhenOrderIsNotConfirmed()
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
       
       //Assert
       await act.Should().ThrowAsync<RpcException>("Only confirmed orders can start preparation!");
    }
}