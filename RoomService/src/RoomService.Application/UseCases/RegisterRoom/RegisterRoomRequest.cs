using RoomService.Application.UseCases.RegisterEquipment;

namespace RoomService.Application.UseCases.RegisterRoom;

public record RegisterRoomRequest(
    string Name,
    int Number,
    List<Guid> EquipmentIds
);