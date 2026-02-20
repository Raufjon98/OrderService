using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;
using OrderService.Api.Features.Common.Exceptions;
using OrderService.Api.Infrastructure.Data;
using OrderService.Contracts.Enums;
using OrderService.Contracts.Interfaces;
using PaymentService.Contracts.Account.Events;
using PaymentService.Contracts.Transaction.Enum;

namespace OrderService.Api.Infrastructure.Consumers.Transactions;

public class WithdrawBalanceConsumer : IConsumer<WithdrawBalanceEvent>
{
    private readonly IOrderService _orderService;
    private readonly OrderDbContext _context;

    public WithdrawBalanceConsumer(IOrderService orderService, OrderDbContext context)
    {
        _orderService = orderService;
        _context = context;
    }

    public async Task Consume(ConsumeContext<WithdrawBalanceEvent> context)
    {
        var orderId = Guid.Parse(context.Message.SourceId);
        if (context.Message.TransactionStatus == TransationStatus.Completed)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o=>o.Id == orderId);
            if (order == null)
            {
                throw new NotFoundException(nameof(Order), context.Message.SourceId);
            }
            order.Status = OrderStatus.Pending;
            await _context.SaveChangesAsync();
        }
    }
}