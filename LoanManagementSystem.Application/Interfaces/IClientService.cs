using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Domain.Entities;

namespace LoanManagementSystem.Application.Interfaces;

public interface IClientService
{
    Task<IEnumerable<Client>> GetClientsAsync(int orgId);
    Task<Client> GetClientByIdAsync(int clientId, int orgId);
    Task<Client> CreateClientAsync(CreateClientRequest request, int orgId);
    Task<Client> UpdateClientAsync(int clientId, UpdateClientRequest request, int orgId);
    Task<bool> SoftDeleteClientAsync(int clientId, int orgId, int currentUserId);
}