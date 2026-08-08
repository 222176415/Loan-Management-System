using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Infrastructure.Services;

namespace LoanManagementSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LoansController(LoanService loanService, ICurrentTenantService tenantService ,  IExcelService excelService ) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLoanRequest request)
    {
        return await ExecuteAsync(async () =>
        {
            var orgId = tenantService.OrganizationId ?? 0;
            if (orgId == 0) throw new Exception("Invalid Organization context.");

            return await loanService.CreateLoanAsync(request, orgId);
        });
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return await ExecuteAsync(async () =>
        {
            var loans = await loanService.GetOrganizationLoansAsync();
        
            return loans.Select(l => new LoanResponse(
                l.Id,
                l.PrincipalAmount,
                l.InterestRate,
                l.TotalAmountDue,
                l.Status.ToString(),
                l.DueDate,
                new ClientResponse( 
                    l.Client.Id, 
                    l.Client.FirstName, 
                    l.Client.Surname, 
                    l.Client.Email, 
                    l.Client.PhoneNumber, 
                    l.Client.Address)
            ));
        });
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateLoanRequest request)
    {
        return await ExecuteAsync(async () =>
        {
          
            return await loanService.UpdateLoanAsync(id, request);
        });
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await ExecuteAsync(async () =>
        {
            // Extract the User ID from the 'sub' claim for the DeletedById field
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                            ?? User.FindFirst("sub")?.Value;
            
            if (!int.TryParse(userIdStr, out var userId))
                throw new Exception("Unable to identify the user performing the deletion.");

            await loanService.SoftDeleteLoanAsync(id, userId);
            
            return "Loan has been successfully soft-deleted and archived.";
        });
    }
    [HttpPut("{id}/default")]
    public async Task<IActionResult> SetDefault(int id) => await ExecuteAsync(async () =>
    {
        await loanService.EscalateToDefaultAsync(id);
        return "Loan contract has been officially flagged as Defaulted.";
    });
    [HttpGet("export")]
    public async Task<IActionResult> ExportLoans()
    {
        var loans = await loanService.GetOrganizationLoansAsync();
    

        var exportData = loans.Select(l => new {
            LoanID = l.Id,
            Client = l.Client != null ? $"{l.Client.FirstName} {l.Client.Surname}" : "N/A",
            Principal = l.PrincipalAmount,
            Interest = $"{l.InterestRate}%",
            TotalDue = l.TotalAmountDue,
            Status = l.Status.ToString(),
            IssueDate = l.IssueDate.ToShortDateString(),
            DueDate = l.DueDate.ToShortDateString()
        });

  
        var fileContent = excelService.ExportToExcel(exportData, "Loans Report");
        
        return File(
            fileContent, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"Loans_Report_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
        );
    }


}