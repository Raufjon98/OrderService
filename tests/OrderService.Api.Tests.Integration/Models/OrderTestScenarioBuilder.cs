using CatalogService.Contracts.Food.Responses;
using MagicOnion;
using NSubstitute;
using OrderService.Contracts.Cart.Requests;
using OrderService.Contracts.CartItem.Requests;
using OrderService.Contracts.Interfaces;
using OrderService.Contracts.Order.Responses;
using PaymentService.Contracts.Account.Responses;

namespace OrderService.Api.Tests.Integration.Models;
public class OrderTestScenarioBuilder
{
    private readonly OrderServiceApiFactory _factory;
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly FoodResponse _food;
    private AccountResponse _account = default!;

    public OrderTestScenarioBuilder(
        OrderServiceApiFactory factory,
        ICartService cartService,
        IOrderService orderService)
    {
        _factory = factory;
        _cartService = cartService;
        _orderService = orderService;

        _food = TestDataFactory.CreateFoodResponse();
        _food.RestaurantId = Guid.NewGuid().ToString();
    }

    public async Task<OrderResponse> CreateOrderAsync()
    {
        _factory.FoodServiceMock
            .GetFoodAsync(_food.Id!)
            .Returns(new UnaryResult<FoodResponse>(_food));

        _account = new AccountResponse
        {
            CustomerId = _userId,
            Balance = 100
        };

        _factory.AccountServiceMock
            .CreateAccountAsync(_userId)
            .Returns(new UnaryResult<AccountResponse>(_account));

        var cartResponse = await _cartService.CreateCartAsync(_userId, new CreateCartRequest
        {
            Items =
            [
                new CartItemRequest
                {
                    FoodId = _food.Id!,
                    Quantity = 3
                }
            ]
        });

        var orderResponse = await _orderService.CreateOrderAsync(_userId);

        return orderResponse;
    }

    public Guid UserId => _userId;
    public string RestaurantId => _food.RestaurantId;
}
