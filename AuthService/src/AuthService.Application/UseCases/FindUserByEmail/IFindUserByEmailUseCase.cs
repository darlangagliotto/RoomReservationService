using AuthService.Domain.Common;

namespace AuthService.Application.UseCases.FindUserByEmail;

public interface IFindUserByEmailUseCase
{
    Task<Result<FindUserByEmailResponse>> ExecuteAsync(FindUserByEmailRequest request);
}