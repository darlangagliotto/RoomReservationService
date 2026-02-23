namespace AuthService.Application.UseCases.RegisterUser
{
    public interface IRegisterUserUseCase
    {
        Task<RegisterUserResponse> ExecuteAsync(RegisterUserRequest request);
    }
}