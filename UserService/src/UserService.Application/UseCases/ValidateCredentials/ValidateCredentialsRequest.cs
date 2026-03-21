namespace UserService.Application.UseCases.ValidateCredentials;

public record ValidateCredentialsRequest(
    string Email,
    string Password
);
