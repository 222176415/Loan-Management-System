using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;

namespace LoanManagementSystem.Infrastructure.Services;

public class CopilotService(IConfiguration configuration) : ICopilotService
{
    private readonly string _connectionString = configuration.GetConnectionString("DbConnection")
        ?? throw new InvalidOperationException("Connection string 'DbConnection' not found.");

    public async Task<DatabaseSchemaDto> GetDatabaseSchemaAsync(int organizationId)
    {
        var schemaMap = new Dictionary<string, List<string>>();

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string query = @"
            SELECT 
                c.TABLE_NAME, 
                c.COLUMN_NAME, 
                c.DATA_TYPE, 
                c.IS_NULLABLE,
                CASE 
                    WHEN k.CONSTRAINT_NAME IS NOT NULL THEN 'PRI'
                    ELSE ''
                END AS COLUMN_KEY
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE k 
                ON c.TABLE_NAME = k.TABLE_NAME 
                AND c.COLUMN_NAME = k.COLUMN_NAME
                AND OBJECTPROPERTY(OBJECT_ID(k.CONSTRAINT_SCHEMA + '.' + k.CONSTRAINT_NAME), 'IsPrimaryKey') = 1
            WHERE c.TABLE_SCHEMA = 'dbo'
            ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION;";

        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            string tableName = reader.GetString(reader.GetOrdinal("TABLE_NAME"));
            string columnName = reader.GetString(reader.GetOrdinal("COLUMN_NAME"));
            string dataType = reader.GetString(reader.GetOrdinal("DATA_TYPE")).ToUpper();
            string isNullable = reader.GetString(reader.GetOrdinal("IS_NULLABLE"));
            string columnKey = reader.GetString(reader.GetOrdinal("COLUMN_KEY"));

            if (!schemaMap.TryGetValue(tableName, out var value))
            {
                value = [];
                schemaMap[tableName] = value;
            }

            string columnDetails = $"{columnName} ({dataType}";
            if (columnKey == "PRI") columnDetails += ", Primary Key";
            if (isNullable == "NO") columnDetails += ", NOT NULL";
            columnDetails += ")";

            value.Add(columnDetails);
        }

        var formattedSchema = string.Join("\n\n", schemaMap.Select(table =>
            $"Table: {table.Key}\n" + string.Join("\n", table.Value.Select(col => $"  - {col}"))
        ));

        return new DatabaseSchemaDto
        {
            OrganizationId = organizationId,
            Database = connection.Database,
            FormattedSchema = formattedSchema,
            Tables = schemaMap
        };
    }
    
    public async IAsyncEnumerable<string> ExecuteAgentPipelineAsync(
        CopilotChatRequest request, 
        int organizationId, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Placeholder streaming output to verify connection with AlexCopilotPanel UI
        yield return "Hello! I am processing your request for ";
        await Task.Delay(200, cancellationToken);
        yield return $"**{request.PageContext.Title}**... ";
        await Task.Delay(200, cancellationToken);
        yield return "Connecting to LLM";
    }
}