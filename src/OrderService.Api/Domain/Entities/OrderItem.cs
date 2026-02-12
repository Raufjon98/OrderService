namespace OrderService.Api.Domain.Entities;

public class OrderItem : BaseAuditableEntity
{
    public required Guid OrderId { get; set; } 
    public Order? Order { get; set; } 
    public required string FoodId { get; set; } 
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}