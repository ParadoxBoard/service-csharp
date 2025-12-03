using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using service_csharp.Data;
using service_csharp.Models;

namespace service_csharp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ParadoxContext _context;

    public CommentsController(ParadoxContext context)
    {
        _context = context;
    }

    // GET: api/Comments?issueId=...
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Comment>>> GetComments([FromQuery] Guid issueId)
    {
        return await _context.Comments
            .Where(c => c.IssueId == issueId)
            .Include(c => c.Author)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    // POST: api/Comments
    [HttpPost]
    public async Task<ActionResult<Comment>> PostComment(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetComments", new { issueId = comment.IssueId }, comment);
    }
    
    // DELETE: api/Comments/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
