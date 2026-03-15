namespace AuthService.Application.UseCases.FindUserById;

public record FindUserByIdResponse(
    Guid Id,
    string Name,
    string Email,
    bool IsBlocked
);