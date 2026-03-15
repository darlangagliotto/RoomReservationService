using AuthService.Domain.Common;
using AuthService.Domain.Repositories;

namespace AuthService.Application.UseCases.FindUserById;

public class FindUserByIdUseCase : IFindUserByIdUseCase
{
    private readonly IUserRepository _userRepository;

    public FindUserByIdUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<FindUserByIdResponse>> ExecuteAsync(FindUserByIdRequest request)
    {
        var user = await _userRepository.GetByIdAsync(request.Id);

        if (user is null)
        {
            return Result<FindUserByIdResponse>.Failure("Usuário não encontrado.");
        }

        return Result<FindUserByIdResponse>.Success(
            new FindUserByIdResponse(
                user.Id,
                user.Name,
                user.Email.Value,
                user.IsBlocked
            )
        );
    }
}