namespace AuthService.Application.DTOs;

public record RegisterUserResponse(
    Guid Id,
    string Name,
    string Email,
    string Token,
    DateTime ExpiresAt
);