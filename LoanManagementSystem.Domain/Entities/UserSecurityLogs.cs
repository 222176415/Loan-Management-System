public class UserSecurityLog
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string ActionType { get; set; } = "PasswordReset";
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = "0.0.0.0";
    public string UserAgent { get; set; } = "Unknown";
}