namespace AuthService.Domain.Services;

public interface IUserValidationService
{
    Task<(bool IsValid, Guid? UserId)> ValidateCredentialsAsync(string email, string password);
}
