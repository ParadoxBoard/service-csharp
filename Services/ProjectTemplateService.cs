using service_csharp.Data;
using service_csharp.Models;

namespace service_csharp.Services;

public interface IProjectTemplateService
{
    List<ProjectTemplate> GetAllTemplates();
    ProjectTemplate? GetTemplateByName(string name);
    Task<Project> CreateProjectFromTemplateAsync(CreateProjectFromTemplateRequest request, Guid createdBy);
}

public class ProjectTemplateService : IProjectTemplateService
{
    private readonly ParadoxContext _context;
    private readonly List<ProjectTemplate> _templates;

    public ProjectTemplateService(ParadoxContext context)
    {
        _context = context;
        _templates = InitializeTemplates();
    }

    public List<ProjectTemplate> GetAllTemplates() => _templates;

    public ProjectTemplate? GetTemplateByName(string name) 
        => _templates.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public async Task<Project> CreateProjectFromTemplateAsync(CreateProjectFromTemplateRequest request, Guid createdBy)
    {
        var template = GetTemplateByName(request.TemplateName);
        if (template == null)
        {
            throw new ArgumentException($"Plantilla '{request.TemplateName}' no encontrada");
        }

        // Crear el proyecto
        var project = new Project
        {
            Name = request.ProjectName,
            Description = request.ProjectDescription ?? template.Description,
            OrgId = request.WorkspaceId, // OrgId es el equivalente a WorkspaceId
            Slug = request.ProjectName.ToLower().Replace(" ", "-"),
            Metadata = System.Text.Json.JsonDocument.Parse($@"{{
                ""methodology"": ""{template.Methodology}"",
                ""template"": ""{template.Name}"",
                ""createdBy"": ""{createdBy}"",
                ""statuses"": {System.Text.Json.JsonSerializer.Serialize(template.DefaultStatuses)},
                ""priorities"": {System.Text.Json.JsonSerializer.Serialize(template.DefaultPriorities)}
            }}")
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Crear el board por defecto
        var board = new Board
        {
            Title = $"{request.ProjectName} - Board",
            ProjectId = project.Id,
            Settings = System.Text.Json.JsonDocument.Parse($@"{{
                ""columns"": {System.Text.Json.JsonSerializer.Serialize(template.DefaultColumns)}
            }}")
        };

        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        return project;
    }

    private List<ProjectTemplate> InitializeTemplates()
    {
        return new List<ProjectTemplate>
        {
            // ══════════════════════════════════════════════════════════════════════════════
            // SCRUM
            // ══════════════════════════════════════════════════════════════════════════════
            new ProjectTemplate
            {
                Name = "Scrum",
                Description = "Metodología Scrum con sprints, backlog y ceremonias",
                Methodology = "Scrum",
                DefaultStatuses = new List<string>
                {
                    "Backlog",
                    "Sprint Backlog",
                    "In Progress",
                    "In Review",
                    "Testing",
                    "Done"
                },
                DefaultPriorities = new List<string>
                {
                    "Critical",
                    "High",
                    "Medium",
                    "Low"
                },
                DefaultColumns = new List<BoardColumnTemplate>
                {
                    new() { Name = "Product Backlog", Order = 1, Color = "#94A3B8" },
                    new() { Name = "Sprint Backlog", Order = 2, Color = "#60A5FA" },
                    new() { Name = "In Progress", Order = 3, Color = "#FBBF24" },
                    new() { Name = "In Review", Order = 4, Color = "#A78BFA" },
                    new() { Name = "Testing", Order = 5, Color = "#F472B6" },
                    new() { Name = "Done", Order = 6, Color = "#34D399" }
                }
            },

            // ══════════════════════════════════════════════════════════════════════════════
            // KANBAN
            // ══════════════════════════════════════════════════════════════════════════════
            new ProjectTemplate
            {
                Name = "Kanban",
                Description = "Metodología Kanban con flujo continuo y límites WIP",
                Methodology = "Kanban",
                DefaultStatuses = new List<string>
                {
                    "To Do",
                    "In Progress",
                    "Review",
                    "Done"
                },
                DefaultPriorities = new List<string>
                {
                    "Urgent",
                    "High",
                    "Normal",
                    "Low"
                },
                DefaultColumns = new List<BoardColumnTemplate>
                {
                    new() { Name = "To Do", Order = 1, Color = "#94A3B8" },
                    new() { Name = "In Progress", Order = 2, Color = "#FBBF24" },
                    new() { Name = "Review", Order = 3, Color = "#A78BFA" },
                    new() { Name = "Done", Order = 4, Color = "#34D399" }
                }
            },

            // ══════════════════════════════════════════════════════════════════════════════
            // XP (Extreme Programming)
            // ══════════════════════════════════════════════════════════════════════════════
            new ProjectTemplate
            {
                Name = "XP",
                Description = "Extreme Programming con énfasis en desarrollo iterativo y calidad",
                Methodology = "XP",
                DefaultStatuses = new List<string>
                {
                    "Story Backlog",
                    "Ready for Dev",
                    "In Development",
                    "Pair Review",
                    "Testing",
                    "Integrated",
                    "Released"
                },
                DefaultPriorities = new List<string>
                {
                    "Must Have",
                    "Should Have",
                    "Could Have",
                    "Won't Have"
                },
                DefaultColumns = new List<BoardColumnTemplate>
                {
                    new() { Name = "Story Backlog", Order = 1, Color = "#94A3B8" },
                    new() { Name = "Ready for Dev", Order = 2, Color = "#60A5FA" },
                    new() { Name = "In Development", Order = 3, Color = "#FBBF24" },
                    new() { Name = "Pair Review", Order = 4, Color = "#A78BFA" },
                    new() { Name = "Testing", Order = 5, Color = "#F472B6" },
                    new() { Name = "Integrated", Order = 6, Color = "#10B981" },
                    new() { Name = "Released", Order = 7, Color = "#34D399" }
                }
            },

            // ══════════════════════════════════════════════════════════════════════════════
            // LEAN
            // ══════════════════════════════════════════════════════════════════════════════
            new ProjectTemplate
            {
                Name = "Lean",
                Description = "Metodología Lean enfocada en eliminar desperdicios y maximizar valor",
                Methodology = "Lean",
                DefaultStatuses = new List<string>
                {
                    "Idea",
                    "Validated",
                    "In Progress",
                    "Delivered",
                    "Measured"
                },
                DefaultPriorities = new List<string>
                {
                    "High Value",
                    "Medium Value",
                    "Low Value"
                },
                DefaultColumns = new List<BoardColumnTemplate>
                {
                    new() { Name = "Idea", Order = 1, Color = "#94A3B8" },
                    new() { Name = "Validated", Order = 2, Color = "#60A5FA" },
                    new() { Name = "In Progress", Order = 3, Color = "#FBBF24" },
                    new() { Name = "Delivered", Order = 4, Color = "#10B981" },
                    new() { Name = "Measured", Order = 5, Color = "#34D399" }
                }
            },

            // ══════════════════════════════════════════════════════════════════════════════
            // SCRUMBAN (Híbrido Scrum + Kanban)
            // ══════════════════════════════════════════════════════════════════════════════
            new ProjectTemplate
            {
                Name = "Scrumban",
                Description = "Híbrido entre Scrum y Kanban con sprints opcionales y flujo continuo",
                Methodology = "Scrumban",
                DefaultStatuses = new List<string>
                {
                    "Backlog",
                    "Ready",
                    "In Progress",
                    "Review",
                    "Done"
                },
                DefaultPriorities = new List<string>
                {
                    "Critical",
                    "High",
                    "Medium",
                    "Low"
                },
                DefaultColumns = new List<BoardColumnTemplate>
                {
                    new() { Name = "Backlog", Order = 1, Color = "#94A3B8" },
                    new() { Name = "Ready", Order = 2, Color = "#60A5FA" },
                    new() { Name = "In Progress", Order = 3, Color = "#FBBF24" },
                    new() { Name = "Review", Order = 4, Color = "#A78BFA" },
                    new() { Name = "Done", Order = 5, Color = "#34D399" }
                }
            }
        };
    }
}
