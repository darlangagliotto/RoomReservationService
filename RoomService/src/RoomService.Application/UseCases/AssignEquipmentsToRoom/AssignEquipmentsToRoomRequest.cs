namespace RoomService.Application.UseCases.AssignEquipmentsToRoom;

public record AssignEquipmentsToRoomRequest(
    Guid RoomId,
    List<Guid> EquipmentIds
);
