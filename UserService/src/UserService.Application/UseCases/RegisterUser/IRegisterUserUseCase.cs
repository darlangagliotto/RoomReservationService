using UserService.Domain.Common;

namespace UserService.Application.UseCases.RegisterUser
{
    public interface IRegisterUserUseCase
    {
        Task<Result<RegisterUserResponse>>  ExecuteAsync(RegisterUserRequest request);
    }
}