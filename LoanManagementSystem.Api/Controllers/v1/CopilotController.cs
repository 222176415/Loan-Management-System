using LoanManagementSystem.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Application.Interfaces;

namespace LoanManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CopilotController(
    ICopilotService copilotService,
    ICurrentTenantService tenantService
) : BaseController
{
    [HttpGet("schema")]
    public async Task<IActionResult> GetDatabaseSchema()
    {
        return await ExecuteAsync(async () =>
        {
            var orgId = tenantService.OrganizationId ?? 0;
            if (orgId == 0) throw new Exception("Invalid Organization context.");

            return await copilotService.GetDatabaseSchemaAsync(orgId);
        });
    }
    [HttpPost("stream")]
    public async Task ProcessCopilotStream([FromBody] CopilotChatRequest request, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        
        // Extract OrganizationId from claims (fallback to DTO context if custom parsed)
        var orgIdClaim = User.FindFirst("OrganizationId")?.Value ?? request.UserContext.OrgId;
        int.TryParse(orgIdClaim, out var organizationId);

        await foreach (var chunk in copilotService.ExecuteAgentPipelineAsync(request, organizationId, cancellationToken))
        {
            await Response.WriteAsync(chunk, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}