using RoomService.Domain.Common;

namespace RoomService.Application.UseCases.AssignEquipmentsToRoom
{
    public interface IAssignEquipmentsToRoomUseCase
    {
        Task<Result<AssignEquipmentsToRoomResponse>> ExecuteAsync(AssignEquipmentsToRoomRequest request);
    }
}
