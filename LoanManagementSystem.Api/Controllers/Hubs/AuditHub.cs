using LoanManagementSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LoanManagementSystem.Api.Controllers.Hubs;

[Authorize]
public class AuditHub(ApplicationDbContext context) : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Check multiple claim casing patterns for robustness
        var orgIdClaim = Context.User?.FindFirst("OrganizationId")?.Value 
                         ?? Context.User?.FindFirst("organizationid")?.Value;
        
        if (int.TryParse(orgIdClaim, out var orgId))
        {
            bool isSuperAdmin = orgId == 1;

            // 1. Join isolated routing partition group channel
            await Groups.AddToGroupAsync(Context.ConnectionId, isSuperAdmin ? "Audit_Global_Admin" : $"Audit_Org_{orgId}");
            
            // 2. Load initial activities (Bypass filter if Organization 1)
            var activities = await context.ActivityLogs
                .Where(x => isSuperAdmin || x.OrganizationId == orgId) 
                .OrderByDescending(x => x.Timestamp)
                .Take(15)
                .ToListAsync();
            await Clients.Caller.SendAsync("LoadInitialActivities", activities);
            
            // 3. Extract tenant email scope for login log mappings
            System.Collections.Generic.List<string>? tenantEmails = null;
            if (!isSuperAdmin)
            {
                tenantEmails = await context.Users
                    .Where(u => u.OrganizationId == orgId)
                    .Select(u => u.Email)
                    .ToListAsync();
            }

            // 4. Load initial logins (Pull all if Super Admin, otherwise filter by tenant emails)
            var logins = await context.UserLoginLogs
                .Where(x => isSuperAdmin || tenantEmails!.Contains(x.UserEmail)) 
                .OrderByDescending(x => x.Timestamp)
                .Take(15)
                .ToListAsync();
            await Clients.Caller.SendAsync("LoadInitialLogins", logins);

            // 5. Fire real-time telemetry metrics
            await StreamTelemetryMetrics(orgId, isSuperAdmin, tenantEmails);
        }
        else
        {
            Context.Abort(); // Teardown context for missing/malformed parameters
        }

        await base.OnConnectedAsync();
    }

    public async Task GetLatestActivities()
    {
        var orgIdClaim = Context.User?.FindFirst("OrganizationId")?.Value 
                         ?? Context.User?.FindFirst("organizationid")?.Value;

        if (int.TryParse(orgIdClaim, out var orgId))
        {
            bool isSuperAdmin = orgId == 1;

            var activities = await context.ActivityLogs
                .Where(x => isSuperAdmin || x.OrganizationId == orgId)
                .OrderByDescending(x => x.Timestamp)
                .Take(10)
                .ToListAsync();
            await Clients.Caller.SendAsync("LoadInitialActivities", activities);
        }
    }
    
    public async Task GetLatestLogins()
    {
        var orgIdClaim = Context.User?.FindFirst("OrganizationId")?.Value 
                         ?? Context.User?.FindFirst("organizationid")?.Value;

        if (int.TryParse(orgIdClaim, out var orgId))
        {
            bool isSuperAdmin = orgId == 1;
            System.Collections.Generic.List<string>? tenantEmails = null;

            if (!isSuperAdmin)
            {
                tenantEmails = await context.Users
                    .Where(u => u.OrganizationId == orgId)
                    .Select(u => u.Email)
                    .ToListAsync();
            }

            var logins = await context.UserLoginLogs
                .Where(x => isSuperAdmin || tenantEmails!.Contains(x.UserEmail))
                .OrderByDescending(x => x.Timestamp)
                .Take(10)
                .ToListAsync();
            await Clients.Caller.SendAsync("LoadInitialLogins", logins);
        }
    }

    public async Task GetSecurityTelemetryMetrics()
    {
        var orgIdClaim = Context.User?.FindFirst("OrganizationId")?.Value 
                         ?? Context.User?.FindFirst("organizationid")?.Value;

        if (int.TryParse(orgIdClaim, out var orgId))
        {
            bool isSuperAdmin = orgId == 1;
            System.Collections.Generic.List<string>? tenantEmails = null;

            if (!isSuperAdmin)
            {
                tenantEmails = await context.Users
                    .Where(u => u.OrganizationId == orgId)
                    .Select(u => u.Email)
                    .ToListAsync();
            }

            await StreamTelemetryMetrics(orgId, isSuperAdmin, tenantEmails);
        }
    }

    #region Real-Time Graph & KPI Telemetry Logic Engine

    private async Task StreamTelemetryMetrics(int orgId, bool isSuperAdmin, System.Collections.Generic.List<string>? tenantEmails)
    {
        var today = DateTime.UtcNow.Date;
        var sevenDaysAgo = today.AddDays(-7);

        // KPI A: Failed logins count (Global vs Tenant-specific)
        var totalFailedLoginsToday = await context.UserLoginLogs
            .Where(x => (isSuperAdmin || tenantEmails!.Contains(x.UserEmail)) && x.Timestamp >= today && !x.IsSuccess)
            .CountAsync();

        // KPI B: System Security Action failures
        var failedSecurityActionsCount = await context.UserSecurityLogs
            .Where(x => (isSuperAdmin || tenantEmails!.Contains(x.UserEmail)) && !x.IsSuccess)
            .CountAsync();

        // KPI C: Count active users matching scope
        var activeOperatorSeats = await context.Users
            .Where(x => isSuperAdmin || (x.OrganizationId == orgId && x.IsActive))
            .CountAsync();

        // Graph Data A: Activity Trend
        var activityTrendData = await context.ActivityLogs
            .Where(x => isSuperAdmin || (x.OrganizationId == orgId && x.Timestamp >= sevenDaysAgo))
            .GroupBy(x => x.Timestamp.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync();

        // Graph Data B: Category Distribution Breakdown
        var logDistribution = await context.ActivityLogs
            .Where(x => isSuperAdmin || x.OrganizationId == orgId)
            .GroupBy(x => x.EntityName)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        var telemetryPayload = new
        {
            KPIs = new {
                FailedLoginsToday = totalFailedLoginsToday,
                ActiveSecurityAlerts = failedSecurityActionsCount,
                ActiveUsersCount = activeOperatorSeats
            },
            Charts = new {
                ActivityTrend = activityTrendData,
                CategoryDistribution = logDistribution
            }
        };

        await Clients.Caller.SendAsync("ReceiveSecurityTelemetry", telemetryPayload);
    }

    #endregion
}