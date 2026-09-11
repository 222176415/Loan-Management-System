namespace LoanManagementSystem.Infrastructure.Services;

public interface ISqlGuardrailService
{
    (bool IsSafe, string FailureReason) ValidateSql(string sql, int organizationId);
}

public class SqlGuardrailService : ISqlGuardrailService
{
    private static readonly string[] ForbiddenKeywords = 
    [
        "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "CREATE", 
        "TRUNCATE", "EXEC", "EXECUTE", "GRANT", "REVOKE", "--", "/*"
    ];

    public (bool IsSafe, string FailureReason) ValidateSql(string sql, int organizationId)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return (false, "Generated SQL was empty.");

        var cleanSql = sql.Trim().ToUpperInvariant();

        // 1. Must start with SELECT or WITH (CTEs)
        if (!cleanSql.StartsWith("SELECT") && !cleanSql.StartsWith("WITH"))
            return (false, "Security Violation: Only SELECT queries are permitted.");

        // 2. Reject destructive or modification keywords
        foreach (var keyword in ForbiddenKeywords)
        {
            if (cleanSql.Contains(keyword))
                return (false, $"Security Violation: Forbidden keyword '{keyword}' detected.");
        }

        // 3. Enforce tenant isolation check where applicable
        if (cleanSql.Contains("ORGANIZATIONID") && !cleanSql.Contains($"ORGANIZATIONID = {organizationId}"))
        {
            return (false, $"Security Violation: Query must filter explicitly by OrganizationId = {organizationId}.");
        }

        return (true, string.Empty);
    }
}