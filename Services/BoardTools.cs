using Microsoft.EntityFrameworkCore;
using service_csharp.Data;
using service_csharp.Models;
using System.ComponentModel;

namespace service_csharp.Services;

/// <summary>
/// Contiene las herramientas (funciones) que la IA puede ejecutar para manipular el tablero.
/// </summary>
public class BoardTools
{
    private readonly ParadoxContext _context;

    public BoardTools(ParadoxContext context)
    {
        _context = context;
    }

    [Description("Crea una nueva tarea o issue en el proyecto especificado.")]
    public async Task<Issue> CreateIssueAsync(
        [Description("El ID del proyecto donde crear la tarea")] Guid projectId,
        [Description("El título de la tarea")] string title,
        [Description("Descripción detallada de la tarea")] string? description = null,
        [Description("Prioridad de la tarea (0: Baja, 1: Media, 2: Alta)")] int? priority = null)
    {
        var issue = new Issue
        {
            ProjectId = projectId,
            Title = title,
            Description = description,
            Priority = priority,
            Status = "Backlog", // Estado por defecto
            Number = _context.Issues.Count(i => i.ProjectId == projectId) + 1
        };

        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();
        return issue;
    }

    [Description("Actualiza el estado de una tarea (mover tarjeta).")]
    public async Task<Issue?> UpdateIssueStatusAsync(
        [Description("El número identificador de la tarea (Issue Number)")] int issueNumber,
        [Description("El ID del proyecto")] Guid projectId,
        [Description("El nuevo estado (ej: 'Backlog', 'Todo', 'In Progress', 'Done')")] string newStatus)
    {
        var issue = await _context.Issues
            .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Number == issueNumber);
            
        if (issue == null) return null;

        issue.Status = newStatus;
        await _context.SaveChangesAsync();
        return issue;
    }

    [Description("Obtiene un resumen de las tareas del proyecto, útil para entender el estado actual.")]
    public async Task<List<object>> GetProjectIssuesSummaryAsync(
        [Description("El ID del proyecto")] Guid projectId,
        [Description("Filtrar por estado opcional")] string? status = null)
    {
        var query = _context.Issues.AsNoTracking().Where(i => i.ProjectId == projectId);
        
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(i => i.Status == status);
        }

        var issues = await query.ToListAsync();

        // Retornamos una proyección simplificada para ahorrar tokens a la IA
        return issues.Select(i => new 
        {
            i.Number,
            i.Title,
            i.Status,
            i.Priority,
            AssignedTo = i.AssigneeId
        }).Cast<object>().ToList();
    }

    [Description("Asigna una tarea a un usuario.")]
    public async Task<Issue?> AssignIssueAsync(
        [Description("El número de la tarea")] int issueNumber,
        [Description("El ID del proyecto")] Guid projectId,
        [Description("El email o username del usuario a asignar")] string userIdentifier)
    {
        var issue = await _context.Issues
            .FirstOrDefaultAsync(i => i.ProjectId == projectId && i.Number == issueNumber);
            
        if (issue == null) return null;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == userIdentifier || u.Username == userIdentifier);

        if (user == null) return null;

        issue.AssigneeId = user.Id;
        await _context.SaveChangesAsync();
        return issue;
    }
}
