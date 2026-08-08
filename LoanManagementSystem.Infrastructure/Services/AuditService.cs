using Microsoft.AspNetCore.Http;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Infrastructure.Persistence;
using LoanManagementSystem.Domain.Entities;
using System.Security.Claims;

namespace LoanManagementSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
public class AuditService(
    ApplicationDbContext context, 
    ICurrentTenantService tenantService,
    IHttpContextAccessor httpContextAccessor) : IAuditService
{
    public async Task LogActivityAsync(string action, string entityName, string entityId, string oldValue = "", string newValue = "")
    {
        // Use the ICurrentTenantService to ensure the log is tied to the correct organization
        var orgId = tenantService.OrganizationId ?? 0;

        var log = new ActivityLog
        {
            OrganizationId = orgId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue,
            Timestamp = DateTime.UtcNow,
            UserId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? "System"
        };

        context.ActivityLogs.Add(log);
        await context.SaveChangesAsync();
    }

    public async Task LogLoginAsync(string email, bool success, string? failureReason = null)
    {
        var contextInfo = httpContextAccessor.HttpContext;

        var log = new UserLoginLog
        {
            UserEmail = email,
            IsSuccess = success,
            FailureReason = failureReason,
            Timestamp = DateTime.UtcNow,
            IpAddress = contextInfo?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0",
            UserAgent = contextInfo?.Request.Headers["User-Agent"].ToString() ?? "Unknown"
        };

        context.UserLoginLogs.Add(log);
        await context.SaveChangesAsync();
    }
    public async Task<IEnumerable<ActivityLog>> GetActivityLogsAsync()
    {
   
        return await context.ActivityLogs
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserLoginLog>> GetLoginLogsAsync()
    {
        return await context.UserLoginLogs
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
    }
    public async Task LogSecurityActionAsync(string email, bool success, string? failureReason = null)
    {
        var contextInfo = httpContextAccessor.HttpContext;

        var log = new UserSecurityLog
        {
            UserEmail = email,
            IsSuccess = success,
            FailureReason = failureReason,
            Timestamp = DateTime.UtcNow,
            IpAddress = contextInfo?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0",
            UserAgent = contextInfo?.Request.Headers["User-Agent"].ToString() ?? "Unknown"
        };

        context.UserSecurityLogs.Add(log);
        await context.SaveChangesAsync();
    }

}