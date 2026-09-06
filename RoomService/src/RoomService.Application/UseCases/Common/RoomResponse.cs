namespace RoomService.Application.UseCases.Common;

public record RoomResponse(
    Guid Id,
    string Name,
    int Number,
    int? PlanSlot,
    List<EquipmentResponse> Equipments
);
