using LoanManagementSystem.Application.DTOs;

namespace LoanManagementSystem.Application.Interfaces;

public interface ICopilotService
{
    Task<DatabaseSchemaDto> GetDatabaseSchemaAsync(int organizationId);
    IAsyncEnumerable<string> ExecuteAgentPipelineAsync(
        CopilotChatRequest request, 
        int organizationId, 
        CancellationToken cancellationToken = default
    );
}

