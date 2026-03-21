using AuthService.Domain.Common;

namespace AuthService.Application.UseCases.LoginUser
{
    public interface ILoginUserUseCase
    {
        Task<Result<LoginUserResponse>> ExecuteAsync(LoginUserRequest request);
    }
}
