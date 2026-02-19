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
using OrderService.Contracts.Order.Responses;
using OrderService.Contracts.OrderItem.Requests;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using PaymentService.Contracts.Account.Responses;

namespace OrderService.Api.Tests.Integration.Endpoints.Orders;

public class AddToOrderTests : IClassFixture<OrderServiceApiFactory>
{
    private readonly OrderServiceApiFactory _factory;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    

    public AddToOrderTests(OrderServiceApiFactory factory)
    {
        _factory = factory;
        var channel = _factory.CreateGrpcChannel();
        _orderService = MagicOnionClient.Create<IOrderService>(channel);
        _cartService = MagicOnionClient.Create<ICartService>(channel);
    }

    [Fact]
    public async Task AddToOrder_AddsItems_WhenDataValid()
    {
        //Arrange
        var scenario = new OrderTestScenarioBuilder(
            _factory,
            _cartService,
            _orderService);
        var userId = scenario.UserId;
        var order = await scenario.CreateOrderAsync();

        var foodToAdd = TestDataFactory.CreateFoodResponse();
        _factory.FoodServiceMock.GetFoodAsync(foodToAdd.Id!).Returns(new UnaryResult<FoodResponse>(foodToAdd));
        foodToAdd.RestaurantId = scenario.RestaurantId;
        
        var itemToAdd = new AddToOrderRequest()
        {
            OrderId = order.Id,
            Items =
            {
                new OrderItemRequest
                {
                    FoodId = foodToAdd.Id!,
                    Quantity = 1
                }
            }
        };
        
        //Act
        var response = await _orderService.AddToOrderAsync(userId, itemToAdd);
        
        //Assert
        response.Should().BeOfType<OrderResponse>();
        response.Id.Should().Be(order.Id);
        response.Items.Should().Contain(i=> i.FoodId == foodToAdd.Id!);
    }

    [Fact]
    public async Task AddToOrder_ThrowsException_WhenOrderNotFound()
    {
        //Arrange
        var userId = Guid.NewGuid();
        var inValidOrderId = Guid.NewGuid();
        var request = new AddToOrderRequest()
        {
            OrderId = inValidOrderId,
            Items =
            {
                new OrderItemRequest
                {
                    FoodId = Guid.NewGuid().ToString(),
                    Quantity = 1
                }
            }
        };
        
        //Act
        Func<Task> act = async () => await _orderService.AddToOrderAsync(userId, request);
        
        //Assert
        await act.Should().ThrowAsync<RpcException>($"Entity Order with key {inValidOrderId} not found!");
    }
}