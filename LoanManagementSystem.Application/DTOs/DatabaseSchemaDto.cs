namespace LoanManagementSystem.Application.DTOs;

public class DatabaseSchemaDto
{
    public int OrganizationId { get; set; }
    public string Database { get; set; } = string.Empty;
    public string FormattedSchema { get; set; } = string.Empty;
    public Dictionary<string, List<string>> Tables { get; set; } = [];
}

public record UserContextDto(
    string Name,
    string Role,
    string Email,
    string OrgId,
    string OrgName
);

public record PageContextDto(
    string Title,
    string Icon,
    string DbNode
);

public record CopilotChatRequest(
    string Prompt,
    List<CopilotChatMessage> Messages,
    UserContextDto UserContext,
    PageContextDto PageContext
);

public record CopilotChatMessage(
    string Sender,
    string Text
);

