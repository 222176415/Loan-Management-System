using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Services;

public class RoleService(ApplicationDbContext context, IAuditService auditService)
{
    public async Task<Role> CreateRoleAsync(string name)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var role = new Role { Name = name };
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            await auditService.LogActivityAsync("CREATE", "Role", role.Id.ToString(), 
                newValue: $"New System Role created: {name}");

            await transaction.CommitAsync();
            return role;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Role creation failed: {ex.Message}");
        }
    }

    public async Task UpdateRoleAsync(int id, string name)
    {
        var role = await context.Roles.FindAsync(id) ?? throw new KeyNotFoundException("Role not found.");
        string oldVal = role.Name;
        
        role.Name = name;
        await context.SaveChangesAsync();

        await auditService.LogActivityAsync("UPDATE", "Role", id.ToString(), oldVal, name);
    }
}