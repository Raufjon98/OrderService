using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Api.Domain.Entities;

namespace OrderService.Api.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o=> o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(o=>o.CustomerId).IsRequired();
        builder.HasIndex(o => o.CustomerId);
        builder.Ignore(o => o.TotalAmount);
        
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(ci => ci.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}