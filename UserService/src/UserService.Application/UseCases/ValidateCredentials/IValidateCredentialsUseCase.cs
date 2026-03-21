using UserService.Domain.Common;

namespace UserService.Application.UseCases.ValidateCredentials;

public interface IValidateCredentialsUseCase
{
    Task<Result<ValidateCredentialsResponse>> ExecuteAsync(ValidateCredentialsRequest request);
}
