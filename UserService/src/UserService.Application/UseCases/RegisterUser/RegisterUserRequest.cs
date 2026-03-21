namespace UserService.Application.UseCases.RegisterUser;

public record RegisterUserRequest(
    string Name,
    string Email,
    string Password
);