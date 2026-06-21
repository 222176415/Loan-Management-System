using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Interfaces;

public interface IAuditService
{
    Task LogActivityAsync(string action, string entityName, string entityId, string oldValue = "", string newValue = "");
    Task LogLoginAsync(string email, bool success, string? failureReason = null);
    
    Task<IEnumerable<ActivityLog>> GetActivityLogsAsync();
    Task<IEnumerable<UserLoginLog>> GetLoginLogsAsync();
}