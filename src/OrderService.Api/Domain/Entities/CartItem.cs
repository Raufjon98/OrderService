namespace OrderService.Api.Domain.Entities;

public class CartItem : BaseAuditableEntity
{
    public required Guid CartId { get; set; }
    public Cart? Cart { get; set; } 
    public required string FoodId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } 
    public decimal Subtotal => Quantity * UnitPrice;
}