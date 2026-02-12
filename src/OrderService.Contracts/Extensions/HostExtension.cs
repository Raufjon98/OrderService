using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Contracts.Interfaces;

namespace OrderService.Contracts.Extensions;

public static class HostExtension
{
    public static IServiceCollection AddOrderServiceContracts(this IServiceCollection services)
    {
        var orderServiceApiUrl = "https://localhost:5021";
        services.AddSingleton<ICartService>(_ =>
            MagicOnionClient.Create<ICartService>(GrpcChannel.ForAddress(orderServiceApiUrl)));
        services.AddSingleton<IOrderService>(_ =>
            MagicOnionClient.Create<IOrderService>(GrpcChannel.ForAddress(orderServiceApiUrl)));

    return services;
    }
}