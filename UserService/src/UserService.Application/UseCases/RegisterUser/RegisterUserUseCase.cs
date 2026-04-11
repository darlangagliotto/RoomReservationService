using UserService.Domain.Common;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Domain.Security;
using UserService.Domain.ValueObjects;

namespace UserService.Application.UseCases.RegisterUser
{
    public class RegisterUserUseCase : IRegisterUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;        

        public RegisterUserUseCase(
            IUserRepository userRepository, 
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<RegisterUserResponse>> ExecuteAsync(RegisterUserRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser is not null)
            {
                return Result<RegisterUserResponse>
                    .Failure("Email is already registered!");
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