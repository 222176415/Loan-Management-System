using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Infrastructure.Persistence;
using LoanManagementSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RolesController(
    RoleService roleService, 
    ApplicationDbContext context, 
    ICurrentTenantService tenantService) : BaseController
{
    private const int MainBranchId = 1;

    [HttpGet]
    public async Task<IActionResult> GetAll() => await ExecuteAsync(async () =>
    {
        return await context.Roles
            .Select(r => new RoleResponse(r.Id, r.Name))
            .ToListAsync();
    });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] string name) => await ExecuteAsync(async () =>
    {
        if (tenantService.OrganizationId != MainBranchId)
            throw new UnauthorizedAccessException("Only Main Branch can create system roles.");

        return await roleService.CreateRoleAsync(name);
    });

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] string name) => await ExecuteAsync(async () =>
    {
        if (tenantService.OrganizationId != MainBranchId)
            throw new UnauthorizedAccessException("Only Main Branch can update system roles.");

        await roleService.UpdateRoleAsync(id, name);
        return "Role updated successfully.";
    });
}