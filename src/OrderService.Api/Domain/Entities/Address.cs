namespace OrderService.Api.Domain.Entities;

public class Address : BaseAuditableEntity
{
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string House { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

