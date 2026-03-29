using RoomService.Application.UseCases.RegisterEquipment;

namespace RoomService.Application.UseCases.RegisterRoom;

public record RegisterRoomResponse(
    Guid Id,
    string Name,
    int Number,
    List<RegisterEquipmentResponse> Equipments
);