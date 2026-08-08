namespace LoanManagementSystem.Domain.Entities;

public class UserLoginLog
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string UserAgent { get; set; } = string.Empty; 
}