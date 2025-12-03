namespace service_csharp.Models;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, Guid UserId, string Email);

public record RegisterRequest(string Email, string Password, string Name);
