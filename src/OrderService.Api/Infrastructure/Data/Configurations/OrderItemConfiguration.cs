using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Api.Domain.Entities;

namespace OrderService.Api.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderId).IsRequired();
        builder.Property(o => o.FoodId)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(o=>o.Quantity).IsRequired();
        builder.Property(o=>o.UnitPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
        builder.Ignore(o => o.Subtotal);
        
        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}