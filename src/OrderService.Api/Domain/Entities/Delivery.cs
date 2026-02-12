using OrderService.Contracts.Enums;

namespace OrderService.Api.Domain.Entities;

public class Delivery : BaseAuditableEntity
{
    public required Guid OrderId { get; set; }
    public Order? Order { get; set; } 
    public required Guid AddressId { get; set; }
    public Address? DeliveryAddress { get; set; }
    public DeliveryStatus Status { get; set; }
}