using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using service_csharp.Data;
using service_csharp.Models;
using service_csharp.Services;

namespace service_csharp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ParadoxContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(ParadoxContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Login de usuario - Genera un token JWT
    /// </summary>
    /// <remarks>
    /// Por ahora, este es un endpoint de prueba que valida contra usuarios existentes en la BD.
    /// En producción deberías implementar hash de contraseñas (BCrypt, Argon2, etc.)
    /// </remarks>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // TODO: Implementar validación real de contraseñas con hash
        // Por ahora, buscamos el usuario por email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized(new { message = "Credenciales inválidas" });
        }

        // Generar token JWT
        var token = _jwtService.GenerateToken(user.Id, user.Email);

        return Ok(new LoginResponse(token, user.Id, user.Email));
    }

    /// <summary>
    /// Endpoint de prueba para verificar autenticación
    /// </summary>
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public ActionResult<object> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        return Ok(new { userId, email, message = "Token válido" });
    }
}
