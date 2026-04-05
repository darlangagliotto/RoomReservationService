using RoomService.Application.UseCases.Common;
using RoomService.Domain.Common;

namespace RoomService.Application.UseCases.GetRooms
{
    public interface IGetRoomsUseCase
    {
        Task<Result<List<RoomResponse>>> ExecuteAsync(GetRoomsRequest request);
    }
}
