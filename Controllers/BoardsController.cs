using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using service_csharp.Data;
using service_csharp.Models;

namespace service_csharp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardsController : ControllerBase
{
    private readonly ParadoxContext _context;

    public BoardsController(ParadoxContext context)
    {
        _context = context;
    }

    // GET: api/Boards
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Board>>> GetBoards()
    {
        return await _context.Boards.Include(b => b.Project).ToListAsync();
    }

    // POST: api/Boards
    [HttpPost]
    public async Task<ActionResult<Board>> PostBoard(Board board)
    {
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetBoards", new { id = board.Id }, board);
    }
}
