namespace UserService.Application.UseCases.ValidateCredentials;

public record ValidateCredentialsResponse(
    bool IsValid,
    Guid? UserId
);
