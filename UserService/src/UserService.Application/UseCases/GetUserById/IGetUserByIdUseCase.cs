using UserService.Domain.Common;

namespace UserService.Application.UseCases.GetUserById
{
    public interface IGetUserByIdUseCase
    {
        Task<Result<GetUserByIdResponse>> ExecuteAsync(GetUserByIdRequest request);
    }
}
