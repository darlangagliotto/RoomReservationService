namespace AuthService.Application.UseCases.FindUserByEmail;

public record FindUserByEmailResponse(
    Guid Id,
    string Name,
    string Email,
    bool IsBlocked
);