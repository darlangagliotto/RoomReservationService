using RoomService.Application.UseCases.Common;

namespace RoomService.Application.UseCases.Common;

public record RoomResponse(
    Guid Id,
    string Name,
    int Number,
    List<EquipmentResponse> Equipments
);