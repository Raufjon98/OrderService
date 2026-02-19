using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Api.Domain.Entities;

namespace OrderService.Api.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c=>c.CustomerId).IsRequired();
        builder.HasIndex(c => c.CustomerId);
        builder.Ignore(c => c.TotalAmount);
        builder.HasQueryFilter(o => !o.IsDeleted);
        
        builder.HasMany(c=>c.Items)
            .WithOne(i=>i.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);;
    }
}