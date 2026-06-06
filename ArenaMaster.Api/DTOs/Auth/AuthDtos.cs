namespace ArenaMaster.Api.DTOs.Auth;

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthUserDto(Guid Id, string Username, string Email, string Role, string? AvatarUrl, bool EmailConfirmed);
