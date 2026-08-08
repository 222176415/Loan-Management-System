using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LoanManagementSystem.Infrastructure.Persistence;
using LoanManagementSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Application.Interfaces;

namespace LoanManagementSystem.Infrastructure.Workers;

public class ChronosWorker(IServiceProvider serviceProvider, ILogger<ChronosWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Chronos Loan Expiry Monitor has initialized.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Run evaluation once every 24 hours
                await EvaluateExpiredLoansAsync();
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred in Chronos Worker: {ex.Message}");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Retry sooner on failure
            }
        }
    }

    private async Task EvaluateExpiredLoansAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

        // Disable Global Filters to process ALL expired loans across ALL organizations simultaneously
        var expiredLoans = await context.Loans
            .IgnoreQueryFilters()
            .Where(l => l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow && !l.IsDeleted)
            .ToListAsync();

        if (expiredLoans.Count == 0) return;

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            foreach (var loan in expiredLoans)
            {
                loan.Status = LoanStatus.Overdue;
                
                string descriptiveAlert = $"Loan ID: {loan.Id} automatically escalated to OVERDUE status by system scheduler. " +
                                           $"Due Date was: {loan.DueDate:yyyy-MM-dd}. Principal: {loan.PrincipalAmount}";
                
                // Write audit row linked to the respective tenant org
                await auditService.LogActivityAsync("SYSTEM_ESCALATION", "Loan", loan.Id.ToString(), "Active", descriptiveAlert);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            logger.LogInformation($"Chronos cleanly migrated {expiredLoans.Count} loans to Overdue status.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError($"Chronos batch transaction failed and rolled back: {ex.Message}");
        }
    }
}
