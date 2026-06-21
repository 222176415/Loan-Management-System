using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Infrastructure.Persistence;
using LoanManagementSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PaymentsController(PaymentService paymentService ,   ApplicationDbContext context) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request) => await ExecuteAsync(async () =>
    {
        return await paymentService.ProcessPaymentAsync(request);
    });

    [HttpGet("loan/{loanId}")]
    public async Task<IActionResult> GetByLoan(int loanId)
    {
        return await ExecuteAsync(async () =>
        {
         
            var loanExists = await context.Loans.AnyAsync(l => l.Id == loanId);
            if (!loanExists)
                throw new KeyNotFoundException("Loan not found.");

            return await paymentService.GetPaymentsByLoanAsync(loanId);
        });
    }

}