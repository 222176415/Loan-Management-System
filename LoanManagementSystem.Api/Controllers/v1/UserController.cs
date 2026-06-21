using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LoanManagementSystem.Infrastructure.Persistence;

namespace LoanManagementSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController(
    UserService userService, 
    ApplicationDbContext context, 
    ICurrentTenantService tenantService, IExcelService excelService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => await ExecuteAsync(async () =>
    {
        return await context.Users
            .Include(u => u.Role)
            .Select(u => new UserResponse(u.Id, u.FullName, u.Email, u.RoleId, u.Role.Name, u.IsActive))
            .ToListAsync();
    });

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request) => await ExecuteAsync(async () =>
    {
        var orgId = tenantService.OrganizationId ?? 0;
        return await userService.CreateUserAsync(request, orgId);
    });

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request) => await ExecuteAsync(async () =>
    {
        return await userService.UpdateUserAsync(id, request);
    });

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) => await ExecuteAsync(async () =>
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        await userService.SoftDeleteUserAsync(id, currentUserId);
        return "User soft-deleted successfully.";
    });
    [HttpPut("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] string newPassword) => await ExecuteAsync(async () =>
    {
       
        var userRoleClaim = User.FindFirst("RoleId")?.Value;
        if (userRoleClaim != "1") 
            throw new UnauthorizedAccessException("Privilege escalation denied. Only organizational administrators can reset user credentials.");

        await userService.ResetUserPasswordAsync(id, newPassword);
        return "User account password credentials have been successfully updated.";
    });
    [HttpGet("export")]
    public async Task<IActionResult> ExportUsers()
    {
        return await ExecuteAsync(async () =>
        {
            var users = await userService.GetOrganizationUsersAsync();
            var exportData = users.Select(u => new {
                ID = u.Id,
                Name = u.FullName,
                Email = u.Email,
                Role = u.Role.Name,
                Status = u.IsActive ? "Active" : "Deactivated",
                Organization = u.Organization.Name,
                DateJoined = u.CreatedAt.ToString("yyyy-MM-dd")
            });

            var fileContent = excelService.ExportToExcel(exportData, "Staff Directory");

            return File(
                fileContent,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Staff_Export_{DateTime.Now:yyyyMMdd}.xlsx"
            );
           
        });
    }

}