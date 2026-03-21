using UserService.Domain.Common;
using UserService.Domain.Repositories;
using UserService.Domain.Security;

namespace UserService.Application.UseCases.ValidateCredentials;

public class ValidateCredentialsUseCase : IValidateCredentialsUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ValidateCredentialsUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<ValidateCredentialsResponse>> ExecuteAsync(ValidateCredentialsRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null)
        {
            return Result<ValidateCredentialsResponse>.Success(
                new ValidateCredentialsResponse(false, null)
            );
        }

        if (user.IsBlocked)
        {
            return Result<ValidateCredentialsResponse>.Success(
                new ValidateCredentialsResponse(false, null)
            );
        }

        var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return Result<ValidateCredentialsResponse>.Success(
                new ValidateCredentialsResponse(false, null)
            );
        }

        return Result<ValidateCredentialsResponse>.Success(
            new ValidateCredentialsResponse(true, user.Id)
        );
    }
}
