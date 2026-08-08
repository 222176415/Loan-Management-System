using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Security;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Services;

public class UserService(ApplicationDbContext context, IAuditService auditService)
{
    public async Task<User> CreateUserAsync(CreateUserRequest request, int orgId)
    {
        var roleExists = await context.Roles.AnyAsync(r => r.Id == request.RoleId);
        if (!roleExists)
        {
            throw new KeyNotFoundException($"Role ID {request.RoleId} is invalid. Please select a valid system role.");
        }

    
        var emailExists = await context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == request.Email);
        if (emailExists)
        {
            throw new InvalidOperationException($"The email address '{request.Email}' is already registered in the system.");
        }

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                OrganizationId = orgId,
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                RoleId = request.RoleId,
                IsActive = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            await auditService.LogActivityAsync("CREATE", "User", user.Id.ToString(), 
                newValue: $"User {user.FullName} ({user.Email}) created with RoleID: {user.RoleId}");

            await transaction.CommitAsync();
            return user;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"User creation failed: {ex.Message}");
        }
    }

    public async Task<User> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await context.Users.FindAsync(id) ?? throw new KeyNotFoundException("User not found.");

        string oldVal = $"Name: {user.FullName}, Role: {user.RoleId}, Active: {user.IsActive}";
        
        user.FullName = request.FullName;
        user.RoleId = request.RoleId;
        user.IsActive = request.IsActive;

        await context.SaveChangesAsync();
        
        string newVal = $"Name: {user.FullName}, Role: {user.RoleId}, Active: {user.IsActive}";
        await auditService.LogActivityAsync("UPDATE", "User", id.ToString(), oldVal, newVal);
        
        return user;
    }

    public async Task SoftDeleteUserAsync(int id, int deletedBy)
    {
        var user = await context.Users.FindAsync(id) ?? throw new KeyNotFoundException("User not found.");
        
        user.IsDeleted = true;
        user.DeletedById = deletedBy;
        user.DeletedAt = DateTime.UtcNow;

        await auditService.LogActivityAsync("DELETE", "User", id.ToString(), 
            newValue: $"User {user.FullName} deactivated/deleted by ID {deletedBy}");

        await context.SaveChangesAsync();
    }

    public async Task ResetUserPasswordAsync(int userId, string newPlainPassword)
    {
        var user = await context.Users.FindAsync(userId) 
                   ?? throw new KeyNotFoundException("The requested staff user context was not found.");

        user.PasswordHash = PasswordHasher.HashPassword(newPlainPassword);
    
        await auditService.LogActivityAsync("PASSWORD_RESET", "User", user.Id.ToString(), 
            newValue: $"Administrative password override executed for user identity: {user.Email}");
        
        await context.SaveChangesAsync();
    }
    public async Task<IEnumerable<User>> GetOrganizationUsersAsync()
    {
        
        return await context.Users
            .Include(u => u.Role)
            .Include(u => u.Organization)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

}
