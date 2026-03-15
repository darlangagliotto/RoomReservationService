using AuthService.Domain.Common;
using AuthService.Domain.Repositories;

namespace AuthService.Application.UseCases.FindUserByEmail;

public class FindUserByEmailUseCase : IFindUserByEmailUseCase
{
    private readonly IUserRepository _userRepository;

    public FindUserByEmailUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<FindUserByEmailResponse>> ExecuteAsync(FindUserByEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<FindUserByEmailResponse>.Failure("É necessário informar o e-mail.");
        }

        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null)
        {
            return Result<FindUserByEmailResponse>.Failure("Usuário não encontrado.");
        }

        return Result<FindUserByEmailResponse>.Success(
            new FindUserByEmailResponse(
                user.Id,
                user.Name,
                user.Email.Value,
                user.IsBlocked
            )
        );
    }
}