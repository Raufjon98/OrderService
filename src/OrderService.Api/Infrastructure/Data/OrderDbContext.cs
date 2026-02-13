using Microsoft.EntityFrameworkCore;
using OrderService.Api.Domain.Entities;

namespace OrderService.Api.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>(); 
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {   
            entity.HasKey(o => o.Id);
            entity.HasQueryFilter(o => !o.IsDeleted);
            entity.HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(ci => ci.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(c=>c.Id);
            entity.HasQueryFilter(o => !o.IsDeleted);
            entity.HasMany(c=>c.Items)
                .WithOne(i=>i.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);;
        });

        modelBuilder.Entity<Delivery>(entity =>
        {   
            entity.HasKey(d => d.Id);
            entity.HasQueryFilter(o => !o.IsDeleted);
            entity.HasOne(d => d.Order)
                .WithOne()
                .HasForeignKey<Delivery>(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<OrderItem>(entity =>
        {   
            entity.HasKey(o => o.Id);
            entity.HasQueryFilter(o => !o.IsDeleted);
        });
        
        modelBuilder.Entity<CartItem>(entity =>
        {   
            entity.HasKey(c => c.Id);
            entity.HasQueryFilter(c => !c.IsDeleted);
        });
    }
}