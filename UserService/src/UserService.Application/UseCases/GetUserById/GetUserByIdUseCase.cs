using UserService.Domain.Common;
using UserService.Domain.Repositories;

namespace UserService.Application.UseCases.GetUserById
{
    public class GetUserByIdUseCase : IGetUserByIdUseCase
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<GetUserByIdResponse>> ExecuteAsync(GetUserByIdRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.Id);

            if (user is null)
            {
                return Result<GetUserByIdResponse>.Failure("Usuário não encontrado.");
            }

            return Result<GetUserByIdResponse>.Success(
                new GetUserByIdResponse(
                    user.Id,
                    user.Name,
                    user.Email.Value
                )
            );
        }
    }
}
