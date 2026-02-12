namespace OrderService.Api.Domain.Entities;

public class Cart : BaseAuditableEntity
{
    public required Guid CustomerId { get; set; }
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    public decimal TotalAmount => Items.Sum(i => i.Subtotal);
}