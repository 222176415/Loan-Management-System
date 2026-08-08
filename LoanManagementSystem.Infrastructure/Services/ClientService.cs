using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.Enums;
using LoanManagementSystem.Infrastructure.Persistence;

namespace LoanManagementSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

public class ClientService(ApplicationDbContext context, IAuditService auditService): IClientService
{
    public async Task<IEnumerable<Client>> GetClientsAsync(int orgId)
    {
        return await context.Clients
            .Where(c => c.OrganizationId == orgId && !c.IsDeleted)
            .AsNoTracking() 
            .ToListAsync();
    }

    public async Task<Client> GetClientByIdAsync(int clientId, int orgId)
    {
        return await context.Clients
                   .AsNoTracking()
                   .FirstOrDefaultAsync(c => c.Id == clientId && c.OrganizationId == orgId && !c.IsDeleted)
               ?? throw new Exception("Client not found or access denied.");
    }
    public async Task<Client> CreateClientAsync(CreateClientRequest request, int orgId)
    {
  
        await ValidateNewClientAsync(request.Email, orgId);

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var client = new Client
            {
                OrganizationId = orgId,
                FirstName = request.FirstName,
                Surname = request.Surname,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                IsDeleted = false
            };

            context.Clients.Add(client);
            await context.SaveChangesAsync();

            string description = $"New client registered: {client.FirstName} {client.Surname} (Email: {client.Email}, Phone: {client.PhoneNumber}) under OrganizationID: {orgId}.";
            await auditService.LogActivityAsync("CREATE", "Client", client.Id.ToString(), newValue: description);

            await transaction.CommitAsync();
            return client;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Client> UpdateClientAsync(int clientId, UpdateClientRequest request, int orgId)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var client = await context.Clients
                .FirstOrDefaultAsync(c => c.Id == clientId && c.OrganizationId == orgId && !c.IsDeleted)
                ?? throw new Exception("Client not found or access denied.");

            string originalDetails = $"Name: {client.FirstName} {client.Surname}, Email: {client.Email}, Phone: {client.PhoneNumber}";

            client.FirstName = request.FirstName;
            client.Surname = request.Surname;
            client.Email = request.Email;
            client.PhoneNumber = request.PhoneNumber;
            client.Address = request.Address;

            await context.SaveChangesAsync();

            string updatedDetails = $"Name: {client.FirstName} {client.Surname}, Email: {client.Email}, Phone: {client.PhoneNumber}";
            await auditService.LogActivityAsync("UPDATE", "Client", client.Id.ToString(), oldValue: originalDetails, newValue: updatedDetails);

            await transaction.CommitAsync();
            return client;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> SoftDeleteClientAsync(int clientId, int orgId, int currentUserId)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var client = await context.Clients
                .FirstOrDefaultAsync(c => c.Id == clientId && c.OrganizationId == orgId && !c.IsDeleted)
                ?? throw new Exception("Client not found or already deleted.");

            // Optional Validation: Ensure client has no active/overdue loans before deleting
            // await EnsureNoActiveLoansAsync(clientId);

            client.IsDeleted = true;
            client.DeletedById = currentUserId;
            client.DeletedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            string description = $"Client {client.FirstName} {client.Surname} (ID: {client.Id}) was soft-deleted by User ID {currentUserId}.";
            await auditService.LogActivityAsync("DELETE", "Client", client.Id.ToString(), newValue: description);

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task ValidateNewClientAsync(string email, int orgId)
    {
        var exists = await context.Clients.AnyAsync(c => c.Email == email && c.OrganizationId == orgId && !c.IsDeleted);
        if (exists)
        {
            throw new Exception("A client with this email already exists within your organization.");
        }
    }
}