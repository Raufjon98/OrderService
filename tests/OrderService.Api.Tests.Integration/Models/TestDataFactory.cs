using CatalogService.Contracts.Food.Responses;
using OrderService.Contracts.Cart.Responses;
using OrderService.Contracts.Order.Responses;

namespace OrderService.Api.Tests.Integration.Models;

public static class TestDataFactory
{
    public static FoodResponse CreateFoodResponse()
    {
       return new FoodResponse
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Food",
            Price = 10,
            Stock = 50,
            RestaurantId = Guid.NewGuid().ToString(),
            FoodCategoryId = Guid.NewGuid().ToString(),
        };
    }
}