using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.Enums;
using LoanManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Services;

public class PaymentService(ApplicationDbContext context, IAuditService auditService)
{
    public async Task<Payment> ProcessPaymentAsync(CreatePaymentRequest request)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // 1. Fetch the Loan (Global Filter ensures Org isolation)
            var loan = await context.Loans
                .Include(l => l.Payments)
                .FirstOrDefaultAsync(l => l.Id == request.LoanId)
                ?? throw new KeyNotFoundException("Loan not found");

            // 2. Validate payment amount
            decimal currentTotalPaid = loan.Payments.Sum(p => p.AmountPaid);
            decimal remainingBalance = loan.TotalAmountDue - currentTotalPaid;

            if (request.AmountPaid > remainingBalance)
                throw new Exception($"Payment exceeds remaining balance. Max allowed: {remainingBalance}");

            // 3. Create the Payment record
            var payment = new Payment
            {
                LoanId = request.LoanId,
                AmountPaid = request.AmountPaid,
                PaymentMethod = request.PaymentMethod,
                PaymentDate = DateTime.UtcNow
            };

            context.Payments.Add(payment);

            // 4. Update Loan Status if fully paid
            bool isFullyPaid = (currentTotalPaid + request.AmountPaid) >= loan.TotalAmountDue;
            if (isFullyPaid)
            {
                loan.Status = LoanStatus.Paid;
            }

            await context.SaveChangesAsync();

            // 5. Descriptive Audit Log
            string logMsg = $"Payment of {request.AmountPaid} received via {request.PaymentMethod}. " +
                            $"Remaining Balance: {remainingBalance - request.AmountPaid}. " +
                            $"Status: {(isFullyPaid ? "Fully Settled" : "Partial")}";

            await auditService.LogActivityAsync("PAYMENT", "Loan", loan.Id.ToString(), newValue: logMsg);

            await transaction.CommitAsync();
            return payment;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Payment processing failed: {ex.Message}");
        }
    }
    
    public async Task<IEnumerable<PaymentResponse>> GetPaymentsByLoanAsync(int loanId)
    {
        try
        {
       
            return await context.Payments
                .Where(p => p.LoanId == loanId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentResponse(
                    p.Id,
                    p.LoanId,
                    p.AmountPaid,
                    p.PaymentDate,
                    p.PaymentMethod
                ))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to retrieve payment history: {ex.Message}");
        }
    }

}
