using AuthService.Domain.Common;
using AuthService.Domain.Repositories;
using AuthService.Domain.Security;

namespace AuthService.Application.UseCases.LoginUser
{
    public class LoginUserUseCase : ILoginUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGenerator _tokenGenerator;

        public LoginUserUseCase(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<Result<LoginUserResponse>> ExecuteAsync(LoginUserRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user is null)
            {
                return Result<LoginUserResponse>.Failure("Usuario nao encontrado!");
            }

            var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Result<LoginUserResponse>.Failure("Senha invalida!");
            }

            var token = _tokenGenerator.Generate(user);

            return Result<LoginUserResponse>.Success(
                new LoginUserResponse(
                    token.Token,
                    token.ExpiresAt
                )
            );
        }
    }
}
