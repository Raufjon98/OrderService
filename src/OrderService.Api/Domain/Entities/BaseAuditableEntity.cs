namespace OrderService.Api.Domain.Entities;

public class BaseAuditableEntity
{
    public Guid Id { get; set; } 
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }

    public BaseAuditableEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
}