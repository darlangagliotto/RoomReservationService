using AuthService.Domain.Common;
using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using AuthService.Domain.Security;
using AuthService.Domain.ValueObjects;

namespace AuthService.Application.UseCases.RegisterUser
{
    public class RegisterUserUseCase : IRegisterUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenGenerator _tokenGenerator;

        public RegisterUserUseCase(
            IUserRepository userRepository, 
            IPasswordHasher passwordHasher,
            ITokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<Result<RegisterUserResponse>> ExecuteAsync(RegisterUserRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser is not null)
            {
                return Result<RegisterUserResponse>
                    .Failure("E-mail já cadastrado!");
            }

            User user;
            try
            {
                user = CreateUser(request);
            }
            catch (DomainException ex)
            {
                return Result<RegisterUserResponse>.Failure(ex.Message);
            }

            await _userRepository.AddSync(user);

            return Result<RegisterUserResponse>.Success(
                new RegisterUserResponse(
                    user.Id,
                    user.Name,
                    user.Email.Value
                )
            );            
        }

        private User CreateUser(RegisterUserRequest request)
        {
            var email = new Email(request.Email);
            var passwordHash = _passwordHasher.Hash(request.Password);
            return new User(request.Name, email, passwordHash);
        }
    }
}