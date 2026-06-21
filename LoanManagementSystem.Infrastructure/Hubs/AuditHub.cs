using LoanManagementSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Hubs;

[Authorize]
public class AuditHub(ApplicationDbContext context) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var orgIdClaim = Context.User?.FindFirst("OrganizationId")?.Value;
        
        if (int.TryParse(orgIdClaim, out var orgId))
        {
            // Join the organization's audit group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Audit_Org_{orgId}");
            
            var activities = await context.ActivityLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(15)
                .ToListAsync();
            await Clients.Caller.SendAsync("LoadInitialActivities", activities);
            
            var logins = await context.UserLoginLogs
                .OrderByDescending(x => x.Timestamp)
                .Take(15)
                .ToListAsync();
            await Clients.Caller.SendAsync("LoadInitialLogins", logins);
        }

        await base.OnConnectedAsync();
    }


    public async Task GetLatestActivities()
    {
        var activities = await context.ActivityLogs
            .OrderByDescending(x => x.Timestamp)
            .Take(10)
            .ToListAsync();
        await Clients.Caller.SendAsync("LoadInitialActivities", activities);
    }
    
    public async Task GetLatestLogins()
    {
        var logins = await context.UserLoginLogs
            .OrderByDescending(x => x.Timestamp)
            .Take(10)
            .ToListAsync();
        await Clients.Caller.SendAsync("LoadInitialLogins", logins);
    }
}