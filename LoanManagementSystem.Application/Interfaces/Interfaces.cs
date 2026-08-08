namespace LoanManagementSystem.Application.Interfaces;


public interface ICurrentTenantService
{
    int? OrganizationId { get; }
}