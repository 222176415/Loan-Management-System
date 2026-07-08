namespace LoanManagementSystem.Application.DTOs;

public record DashboardSummaryResponse(
    DashboardMetrics Metrics,
    List<MonthlyTrendItem> TrendData,
    List<RiskPortfolioItem> RiskData,
    List<RecentActivityLogItem> RecentActivityLogs
);

public record DashboardMetrics(
    string ActiveDisbursed,   // Format: "R 248,500" or similar
    string OverdueAtRisk,
    string CollectedInterest,
    int PendingRequests
);

public record MonthlyTrendItem(
    string Month,              // "Jan", "Feb", etc.
    decimal CapitalIssued,
    decimal RevenueCollected
);

public record RiskPortfolioItem(
    string Name,               // "Active Book", "Overdue Risk", "Defaulted"
    decimal Value
);

public record RecentActivityLogItem(
    int Id,
    string Action,
    string EntityName,
    string EntityId,
    string OldValue,
    string NewValue,
    string UserId,
    DateTime Timestamp
);
public interface IAnalyticsService
{
    Task<DashboardSummaryResponse> GetDashboardSummaryAsync(int orgId, int roleId);
}