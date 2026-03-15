using AuthService.Domain.Common;

namespace AuthService.Application.UseCases.FindUserById;

public interface IFindUserByIdUseCase
{
    Task<Result<FindUserByIdResponse>> ExecuteAsync(FindUserByIdRequest request);
}