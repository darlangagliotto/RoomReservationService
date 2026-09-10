using RoomService.Domain.Common;

namespace RoomService.Application.UseCases.RemoveEquipmentFromRoom
{
    public interface IRemoveEquipmentFromRoomUseCase
    {
        Task<Result<RemoveEquipmentFromRoomResponse>> ExecuteAsync(RemoveEquipmentFromRoomRequest request);
    }
}
