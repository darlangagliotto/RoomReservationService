namespace AuthService.Application.UseCases.RegisterUser;

public record RegisterUserResponse(
    Guid Id,
    string Name,
    string Email
);