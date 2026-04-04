using RoomService.Domain.Common;

namespace RoomService.Application.UseCases.RegisterRoom
{
    public interface IRegisterRoomUseCase
    {
        Task<Result<RegisterRoomResponse>> ExecuteAsync(RegisterRoomRequest request);
    }
}