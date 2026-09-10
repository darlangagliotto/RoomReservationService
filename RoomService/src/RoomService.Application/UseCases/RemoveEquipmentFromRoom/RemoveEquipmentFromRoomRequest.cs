namespace RoomService.Application.UseCases.RemoveEquipmentFromRoom;

public record RemoveEquipmentFromRoomRequest(
    Guid RoomId,
    Guid EquipmentId
);
