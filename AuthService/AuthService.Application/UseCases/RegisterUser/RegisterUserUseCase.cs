using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using AuthService.Domain.Repositories;
using AuthService.Domain.Security;
using AuthService.Domain.ValueObjects;
using AutoMapper;

namespace AuthService.Application.UseCases.RegisterUser
{
    public class RegisterUserUseCase : IRegisterUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;

        public RegisterUserUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher, IMapper mapper)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        public async Task<RegisterUserResponse> ExecuteAsync(RegisterUserRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser is not null)
            {
                throw new InvalidOperationException("E-mail já cadastrado!");
            }

            var user = CreateUser(request);
            await _userRepository.AddSync(user);
            var response = _mapper.Map<RegisterUserResponse>(user);
            return response with
            {
                Token = "fake-jwt-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }

        private User CreateUser(RegisterUserRequest request)
        {
            var email = new Email(request.Email);
            var passwordHash = _passwordHasher.Hash(request.Password);

            return new User(request.Name, email, passwordHash);
        }

    }
}