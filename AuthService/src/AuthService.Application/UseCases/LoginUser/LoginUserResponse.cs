namespace AuthService.Application.UseCases.LoginUser;

public record LoginUserResponse(
    string Token,
    DateTime ExpiresAt
);