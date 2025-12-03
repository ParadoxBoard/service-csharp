namespace service_csharp.Models;

/// <summary>
/// Plantilla de proyecto ágil con configuración predefinida
/// </summary>
public class ProjectTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Methodology { get; set; } = string.Empty; // Scrum, Kanban, etc.
    public List<string> DefaultStatuses { get; set; } = new();
    public List<string> DefaultPriorities { get; set; } = new();
    public List<BoardColumnTemplate> DefaultColumns { get; set; } = new();
}

public class BoardColumnTemplate
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Color { get; set; }
}

/// <summary>
/// Request para crear un proyecto desde una plantilla
/// </summary>
public record CreateProjectFromTemplateRequest(
    string TemplateName,
    string ProjectName,
    string? ProjectDescription,
    Guid WorkspaceId
);
