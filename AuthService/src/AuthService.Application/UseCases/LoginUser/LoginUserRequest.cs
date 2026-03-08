namespace AuthService.Application.UseCases.LoginUser;

public record LoginUserRequest(
    string Email,
    string Password
);