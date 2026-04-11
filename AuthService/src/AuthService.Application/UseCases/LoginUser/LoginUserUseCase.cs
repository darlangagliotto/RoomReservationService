using AuthService.Domain.Common;
using AuthService.Domain.Security;
using AuthService.Domain.Services;

namespace AuthService.Application.UseCases.LoginUser
{
    public class LoginUserUseCase : ILoginUserUseCase
    {
        private readonly IUserValidationService _userValidationService;
        private readonly ITokenGenerator _tokenGenerator;

        public LoginUserUseCase(
            IUserValidationService userValidationService,
            ITokenGenerator tokenGenerator)
        {
            _userValidationService = userValidationService;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<Result<LoginUserResponse>> ExecuteAsync(LoginUserRequest request)
        {
            var (isValid, userId) = await _userValidationService.ValidateCredentialsAsync(request.Email, request.Password);

            if (!isValid || !userId.HasValue)
            {
                return Result<LoginUserResponse>.Failure("Invalid email or password!");
            }

            var token = _tokenGenerator.Generate(userId.Value, request.Email);

            return Result<LoginUserResponse>.Success(
                new LoginUserResponse(
                    token.Token,
                    token.ExpiresAt
                )
            );
        }
    }
}
