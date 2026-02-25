using System.Reflection;
using CatalogService.Contracts.Extensions;
using CustomerService.Contracts.Extensions;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Features.Common.Behaviors;
using OrderService.Api.Features.Jobs;
using OrderService.Api.Infrastructure.Consumers.Transactions;
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
        configuration.AddConsumer<WithdrawBalanceConsumer>();
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
builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseHangfireDashboard("/hangfire", 
    new DashboardOptions
    {
        DashboardTitle = "Hangfire Dashboard", TimeZoneResolver = new DefaultTimeZoneResolver()
    });

RecurringJob.AddOrUpdate<ExpiredCartsCleanUpJob>(
    "CleanExpiredCarts",
    x => x.RemoveExpiredCartsAsync(CancellationToken.None),
    Cron.Weekly(DayOfWeek.Sunday), new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
RecurringJob.AddOrUpdate<ExpiredOrdersCleanUpJob>(
    "CleanExpiredOrders",
    x => x.RemoveExpiredOrdersAsync(CancellationToken.None),
    Cron.Weekly(DayOfWeek.Sunday), new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

app.MapGet("/", () => "OrderService Api");
app.MapMagicOnionService();
app.Run();

