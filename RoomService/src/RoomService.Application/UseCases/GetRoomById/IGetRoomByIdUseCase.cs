using RoomService.Application.UseCases.Common;
using RoomService.Domain.Common;

namespace RoomService.Application.UseCases.GetRoomById
{
    public interface IGetRoomByIdUseCase
    {
        Task<Result<RoomResponse>> ExecuteAsync(GetRoomByIdRequest request);
    }
}
