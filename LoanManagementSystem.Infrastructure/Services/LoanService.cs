using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Domain.Enums;
using LoanManagementSystem.Infrastructure.Persistence;

namespace LoanManagementSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
public class LoanService(ApplicationDbContext context, IAuditService auditService)
{
    public async Task<Loan> CreateLoanAsync(CreateLoanRequest request, int orgId)
    //check for other Pending loans or overdue
    {  await ValidateClientLoanStatusAsync(request.ClientId);
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
        
            var org = await context.Organizations.FindAsync(orgId) 
                      ?? throw new Exception("Organization not found.");

            var loan = new Loan
            {
                OrganizationId = orgId,
                ClientId = request.ClientId,
                PrincipalAmount = request.PrincipalAmount,
                InterestRate = org.DefaultInterestRate, 
                VatRate = org.VatRate,                
                TotalAmountDue = request.PrincipalAmount + (request.PrincipalAmount * (org.DefaultInterestRate / 100)),
                IssueDate = DateTime.UtcNow,
                DueDate = request.DueDate,
                Status = Domain.Enums.LoanStatus.Active
            };

            context.Loans.Add(loan);
            await context.SaveChangesAsync();
            
            string description = $"New {loan.Status} loan created for ClientID: {loan.ClientId}. " +
                                 $"Principal: {loan.PrincipalAmount}, Interest: {loan.InterestRate}%, " +
                                 $"Total Due: {loan.TotalAmountDue}, Due Date: {loan.DueDate:yyyy-MM-dd}";

            await auditService.LogActivityAsync("CREATE", "Loan", loan.Id.ToString(), newValue: description);

            await transaction.CommitAsync();
            return loan;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task<IEnumerable<Loan>> GetOrganizationLoansAsync()
    {
        return await context.Loans
            .Include(l => l.Client) 
            .OrderByDescending(l => l.IssueDate)
            .ToListAsync();
    }
    public async Task<Loan> UpdateLoanAsync(int id, CreateLoanRequest request)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var loan = await context.Loans.Include(l => l.Client)
                           .FirstOrDefaultAsync(l => l.Id == id) 
                       ?? throw new KeyNotFoundException($"Loan Entry not found.");

            string oldValue = $"Principal: {loan.PrincipalAmount}, Due: {loan.DueDate:yyyy-MM-dd}";

            loan.PrincipalAmount = request.PrincipalAmount;
            loan.DueDate = request.DueDate;
            loan.TotalAmountDue = loan.PrincipalAmount + (loan.PrincipalAmount * (loan.InterestRate / 100));

            await context.SaveChangesAsync();

            string newValue = $"Principal: {loan.PrincipalAmount}, TotalDue: {loan.TotalAmountDue}";
            await auditService.LogActivityAsync("UPDATE", "Loan", id.ToString(), oldValue, newValue);

            await transaction.CommitAsync();
            return loan;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Failed to update loan: {ex.Message}");
        }
    }

    public async Task SoftDeleteLoanAsync(int id, int userId)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var loan = await context.Loans.FindAsync(id) 
                       ?? throw new KeyNotFoundException("Loan not found.");

            loan.IsDeleted = true;
            loan.DeletedById = userId;
            loan.DeletedAt = DateTime.UtcNow;

            await auditService.LogActivityAsync("DELETE", "Loan", id.ToString(), newValue: $"Soft deleted by User {userId}");

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Database error during deletion: {ex.Message}");
        }
    }
    private async Task ValidateClientLoanStatusAsync(int clientId)
    {
        var regularOpenLoan = await context.Loans
            .AnyAsync(l => l.ClientId == clientId && 
                           (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));

        if (regularOpenLoan)
        {
            throw new InvalidOperationException("Transaction rejected. This client currently holds an active or overdue loan obligation that must be settled first.");
        }
    }
    public async Task EscalateToDefaultAsync(int loanId)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var loan = await context.Loans.Include(l => l.Client)
                           .FirstOrDefaultAsync(l => l.Id == loanId)
                       ?? throw new KeyNotFoundException("Loan record not found or access unauthorized.");

            if (loan.Status != LoanStatus.Overdue)
                throw new InvalidOperationException("Only open loans that are currently in an 'Overdue' state can be escalated to Defaulted.");

            loan.Status = LoanStatus.Defaulted;
            await context.SaveChangesAsync();

            string descriptiveLog = $"Loan ID: {loan.Id} for Client {loan.Client.FirstName} {loan.Client.Surname} manually declared as DEFAULTED by branch management. Principal loss: {loan.PrincipalAmount}";
            await auditService.LogActivityAsync("ESCALATE_DEFAULT", "Loan", loan.Id.ToString(), "Overdue", descriptiveLog);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Escalation sequence failed: {ex.Message}");
        }
    }


}