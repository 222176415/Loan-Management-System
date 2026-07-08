using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Infrastructure.Services;

namespace LoanManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(IClientService clientService,ICurrentTenantService tenantService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await ExecuteAsync(async () =>
        {
            var orgId = tenantService.OrganizationId ?? 0;
            if (orgId == 0) throw new Exception("Invalid Organization context.");

            return await clientService.GetClientsAsync(orgId);
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        return await ExecuteAsync(async () =>
        {
            var orgId = tenantService.OrganizationId ?? 0;
            if (orgId == 0) throw new Exception("Invalid Organization context.");

            return await clientService.GetClientByIdAsync(id, orgId);
        });
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
    {
        return await ExecuteAsync(async () =>
        {
            var orgId = tenantService.OrganizationId ?? 0;
            if (orgId == 0) throw new Exception("Invalid Organization context.");

            return await clientService.CreateClientAsync(request, orgId);
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientRequest request)
    {
        return await ExecuteAsync(async () =>
        {
            var orgId = tenantService.OrganizationId ?? 0;
            if (orgId == 0) throw new Exception("Invalid Organization context.");

            return await clientService.UpdateClientAsync(id, request, orgId);
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await ExecuteAsync(async () =>
        {
            var orgId = tenantService.OrganizationId ?? 0;
            if (orgId == 0) throw new Exception("Invalid Organization context.");

            // Hardcode or retrieve CurrentUserId from a user context service if available
            int currentUserId = 1; 

            return await clientService.SoftDeleteClientAsync(id, orgId, currentUserId);
        });
    }
}