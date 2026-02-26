using AuthService.Domain.Common;

namespace AuthService.Application.UseCases.RegisterUser
{
    public interface IRegisterUserUseCase
    {
        Task<Result<RegisterUserResponse>>  ExecuteAsync(RegisterUserRequest request);
    }
}