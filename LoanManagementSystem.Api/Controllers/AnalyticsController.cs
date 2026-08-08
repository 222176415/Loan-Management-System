using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController(IAnalyticsService analyticsService, ICurrentTenantService tenantService) : BaseController
{
    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        return await ExecuteAsync(async () =>
        {
            var orgId = tenantService.OrganizationId ?? 0;
            if (orgId == 0) throw new Exception("Invalid Organization context.");

            // Extract the Role ID from your token claims context
            // Assuming your current user context or claims principle holds this value:
            int roleId = int.Parse(User.FindFirst("RoleId")?.Value ?? "0");

            return await analyticsService.GetDashboardSummaryAsync(orgId, roleId);
        });
    }
}