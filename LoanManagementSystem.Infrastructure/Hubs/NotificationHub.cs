using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using LoanManagementSystem.Infrastructure.Persistence;

namespace LoanManagementSystem.Infrastructure.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var orgId = Context.User?.FindFirst("OrganizationId")?.Value;
        
        if (!string.IsNullOrEmpty(orgId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Org_{orgId}");
        }

        await base.OnConnectedAsync();
    }
}