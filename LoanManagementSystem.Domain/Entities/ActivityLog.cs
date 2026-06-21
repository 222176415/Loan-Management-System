namespace LoanManagementSystem.Domain.Entities;

public class ActivityLog
{
    public int Id { get; set; }
    public int OrganizationId { get; set; } 
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; 
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty; 
    public string NewValue { get; set; } = string.Empty; 
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}