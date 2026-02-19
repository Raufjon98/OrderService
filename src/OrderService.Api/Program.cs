using System.Reflection;
using CatalogService.Contracts.Extensions;
using CustomerService.Contracts.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Infrastructure.Data;
using OrderService.Api.Infrastructure.Interceptors;
using OrderService.Api.MagicOnion.Services;
using OrderService.Contracts.Interfaces;
using PaymentService.Contracts.Extentions;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5021, listenOptions =>
    {
        listenOptions.UseHttps();              
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});
var rabbitConnectionString = builder.Configuration["MessageBroker:Host"];

if (!builder.Environment.IsEnvironment("IntegrationTest"))
{
    builder.Services.AddMassTransit(configuration =>
    {
        configuration.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(rabbitConnectionString);
            cfg.ExchangeType = ExchangeType.Fanout;
            cfg.ConfigureEndpoints(ctx);
        });
    });
}

builder.Services.AddOpenApi();
builder.Services.AddDbContext<OrderDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService.Api.MagicOnion.Services.OrderService>();
builder.Services.AddCatalogServiceContracts();
builder.Services.AddCustomerServiceContracts();
builder.Services.AddPaymentServiceContracts();
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ExceptionInterceptor>();
});
builder.Services.AddMagicOnion();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "OrderService Api");
app.MapMagicOnionService();
app.Run();

