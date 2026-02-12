using System.Reflection;
using CatalogService.Contracts.Extensions;
using CustomerService.Contracts.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Infrastructure.Data;
using OrderService.Api.Infrastructure.Interceptors;
using OrderService.Api.MagicOnion.Services;
using OrderService.Contracts.Interfaces;
using PaymentService.Contracts.Extentions;

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

