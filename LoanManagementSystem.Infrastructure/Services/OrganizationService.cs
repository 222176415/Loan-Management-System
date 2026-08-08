using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace LoanManagementSystem.Infrastructure.Services;

public class OrganizationService(ApplicationDbContext context, IAuditService auditService)
{
    public async Task<Organization> CreateAsync(CreateOrganizationRequest request)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try {
            var org = new Organization {
                Name = request.Name,
                Email = request.Email,
                VatRate = request.VatRate,
                DefaultInterestRate = request.DefaultInterestRate
            };
            context.Organizations.Add(org);
            await context.SaveChangesAsync();

            await auditService.LogActivityAsync("ONBOARD", "Organization", org.Id.ToString(), 
                newValue: $"New Organization '{org.Name}' onboarded.");

            await transaction.CommitAsync();
            return org;
        } catch {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(int id, UpdateOrganizationRequest request)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var org = await context.Organizations.FindAsync(id)
                      ?? throw new KeyNotFoundException("Organization not found.");

            string oldVal = $"Vat: {org.VatRate}, Interest: {org.DefaultInterestRate}";
            org.Name = request.Name;
            org.VatRate = request.VatRate;
            org.DefaultInterestRate = request.DefaultInterestRate;
            context.Organizations.Update(org);
await transaction.CommitAsync();    
            await context.SaveChangesAsync();
            await auditService.LogActivityAsync("UPDATE", "Organization", id.ToString(), oldVal,
                $"New Name: {org.Name}");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task SoftDeleteAsync(int id, int userId)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var org = await context.Organizations.FindAsync(id) 
                      ?? throw new KeyNotFoundException($"Organization with ID {id} not found.");
        
            string logDetail = $"Organization '{org.Name}' (Email: {org.Email}) soft-deleted by User ID: {userId}";

               org.IsDeleted = true;
             org.DeletedById = userId; 
            org.DeletedAt = DateTime.UtcNow;
            
            context.Organizations.Update(org);
            await context.SaveChangesAsync();
            await auditService.LogActivityAsync("DELETE", "Organization", id.ToString(), newValue: logDetail);
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Failed to delete organization: {ex.Message}");
        }
    }
    public async Task<IEnumerable<OrganizationResponse>> GetAllOrganizationsAsync(int currentOrgId)
    {
        if (currentOrgId != 1) 
            throw new UnauthorizedAccessException("Access denied. Only the Main Branch can view the global directory.");

       return await context.Organizations
            .Select(o => new OrganizationResponse(o.Id, o.Name, o.Email, o.VatRate, o.DefaultInterestRate))
            .ToListAsync();


    }
}
