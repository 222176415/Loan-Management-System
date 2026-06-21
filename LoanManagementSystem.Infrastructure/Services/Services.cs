using Microsoft.AspNetCore.Http;
using LoanManagementSystem.Application.Interfaces;
using System.Security.Claims;

namespace LoanManagementSystem.Infrastructure.Services;

public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? OrganizationId
    {
        get
        {
            // Using the standard FindFirst method instead of the extension FindFirstValue
            var orgClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("OrganizationId")?.Value;
            
            return int.TryParse(orgClaim, out var id) ? id : null;
        }
    }
}