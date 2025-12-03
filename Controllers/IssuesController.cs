using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using service_csharp.Data;
using service_csharp.Models;

namespace service_csharp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IssuesController : ControllerBase
{
    private readonly ParadoxContext _context;

    public IssuesController(ParadoxContext context)
    {
        _context = context;
    }

    // GET: api/Issues
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Issue>>> GetIssues([FromQuery] Guid? boardId)
    {
        var query = _context.Issues.AsQueryable();

        if (boardId.HasValue)
        {
            query = query.Where(i => i.BoardId == boardId);
        }

        return await query.ToListAsync();
    }

    // POST: api/Issues
    [HttpPost]
    public async Task<ActionResult<Issue>> PostIssue(Issue issue)
    {
        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetIssues", new { id = issue.Id }, issue);
    }
    
    // PUT: api/Issues/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutIssue(Guid id, Issue issue)
    {
        if (id != issue.Id)
        {
            return BadRequest();
        }

        _context.Entry(issue).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
