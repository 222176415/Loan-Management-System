using System.Globalization;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Enums;
using LoanManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Services;

public class AnalyticsService(ApplicationDbContext context) : IAnalyticsService
{
   public async Task<DashboardSummaryResponse> GetDashboardSummaryAsync(int orgId, int roleId)
    {
        var SouthAfricanCulture = new CultureInfo("en-ZA");
        
        // Super admin evaluation rule
        bool isSuperAdmin = (orgId == 1 && roleId == 1);

        // ----------------------------------------------------
        // 1. Build Condition-Driven Base Queries
        // ----------------------------------------------------
        var loansQuery = context.Loans.AsQueryable();
        var clientsQuery = context.Clients.AsQueryable();
        var logsQuery = context.ActivityLogs.AsQueryable();

        if (isSuperAdmin)
        {
            // Bypass tenant filters entirely, but manually keep soft-delete filtering
            loansQuery = loansQuery.IgnoreQueryFilters().Where(l => !l.IsDeleted);
            clientsQuery = clientsQuery.IgnoreQueryFilters().Where(c => !c.IsDeleted);
            logsQuery = logsQuery.IgnoreQueryFilters();
        }
        else
        {
            // Fallback on standard query filters automatically managed by context configurations
            loansQuery = loansQuery.Where(l => l.Status == LoanStatus.Active);
        }

        // ----------------------------------------------------
        // 2. Compute Dashboard Analytics Metrics
        // ----------------------------------------------------
        // If super admin, pull total metrics system-wide; else, target tenant parameters
        decimal totalActivePrincipal = await loansQuery
            .Where(l => isSuperAdmin ? l.Status == LoanStatus.Active : true)
            .SumAsync(l => l.PrincipalAmount);
        
        decimal totalOverdue = await loansQuery
            .Where(l => l.DueDate < DateTime.UtcNow && l.Status == LoanStatus.Active)
            .SumAsync(l => l.TotalAmountDue);

        decimal interestRevenue = await loansQuery
            .Where(l => l.Status == LoanStatus.Active)
            .SumAsync(l => l.TotalAmountDue - l.PrincipalAmount);

        int pendingApprovals = await clientsQuery.CountAsync();

        var metrics = new DashboardMetrics(
            ActiveDisbursed: totalActivePrincipal.ToString("C0", SouthAfricanCulture),
            OverdueAtRisk: totalOverdue.ToString("C0", SouthAfricanCulture),
            CollectedInterest: interestRevenue.ToString("C0", SouthAfricanCulture),
            PendingRequests: pendingApprovals
        );

        // ----------------------------------------------------
        // 3. Historical Deployment Trends (Past 6 Months)
        // ----------------------------------------------------
        var halfYearAgo = DateTime.UtcNow.AddMonths(-6);
        
        var loansData = await loansQuery
            .Where(l => l.IssueDate >= halfYearAgo)
            .Select(l => new { l.IssueDate, l.PrincipalAmount, l.TotalAmountDue })
            .ToListAsync();

        var trendData = loansData
            .GroupBy(l => l.IssueDate.ToString("MMM"))
            .Select(g => new MonthlyTrendItem(
                Month: g.Key,
                CapitalIssued: g.Sum(l => l.PrincipalAmount),
                RevenueCollected: g.Sum(l => l.TotalAmountDue - l.PrincipalAmount)
            )).ToList();

        // ----------------------------------------------------
        // 4. Portfolio Allocation Mappings
        // ----------------------------------------------------
        var riskData = new List<RiskPortfolioItem>
        {
            new("Active Book", totalActivePrincipal),
            new("Overdue Risk", totalOverdue),
            new("Defaulted", totalOverdue * 0.15m) 
        };

        // ----------------------------------------------------
        // 5. Audit Logging Stream Feed (Top 3 items)
        // ----------------------------------------------------
        var recentLogs = await logsQuery
            .OrderByDescending(a => a.Timestamp)
            .Take(3)
            .Select(x => new RecentActivityLogItem(
                x.Id,
                x.Action,
                x.EntityName,
                x.EntityId,
                x.OldValue,
                x.NewValue,
                x.UserId,
                x.Timestamp
            ))
            .ToListAsync();

        return new DashboardSummaryResponse(metrics, trendData, riskData, recentLogs);
    }
}