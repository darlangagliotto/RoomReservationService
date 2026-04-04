using RoomService.Domain.Common;

namespace RoomService.Application.UseCases.GetAllRooms
{
    public interface IGetAllRoomsUseCase
    {
        Task<Result<GetAllRoomsResponse>> ExecuteAsync();
    }
}