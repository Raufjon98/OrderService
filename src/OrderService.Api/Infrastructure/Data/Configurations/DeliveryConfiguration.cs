using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Api.Domain.Entities;

namespace OrderService.Api.Infrastructure.Data.Configurations;

public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.OrderId).IsRequired();
        builder.Property(d => d.AddressId).IsRequired();
        builder.HasQueryFilter(d => !d.IsDeleted);
        builder.HasOne(d => d.Order)
            .WithOne()
            .HasForeignKey<Delivery>(d => d.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}