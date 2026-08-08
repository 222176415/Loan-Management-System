using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanManagementSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrganizationsController(
    OrganizationService orgService, 
    ICurrentTenantService tenantService) : BaseController
{
    private const int MainBranchId = 1;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationRequest request) => await ExecuteAsync(async () =>
    {
        if (tenantService.OrganizationId != MainBranchId)
            throw new UnauthorizedAccessException("Only Main Branch can onboard new organizations.");

        return await orgService.CreateAsync(request);
    });

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrganizationRequest request) => await ExecuteAsync(async () =>
    {
        var currentOrgId = tenantService.OrganizationId;

        // Rule: Super Admin (Main Branch) can update anyone, 
        // ordinary Admins can only update their own ID.
        if (currentOrgId != MainBranchId && currentOrgId != id)
            throw new UnauthorizedAccessException("You do not have permission to update this organization.");

        await orgService.UpdateAsync(id, request);
        return "Organization updated successfully.";
    });

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) => await ExecuteAsync(async () =>
    {
        if (tenantService.OrganizationId != MainBranchId)
            throw new UnauthorizedAccessException("Only Main Branch can perform deletions.");

       
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        await orgService.SoftDeleteAsync(id, userId);
        return "Organization soft-deleted.";
    });
    
    [HttpGet]
    public async Task<IActionResult> GetAll() => await ExecuteAsync(async () =>
    {
        var currentOrgId = tenantService.OrganizationId ?? 0;
        return await orgService.GetAllOrganizationsAsync(currentOrgId);
    });

}