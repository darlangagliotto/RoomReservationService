namespace RoomService.Application.UseCases.GetEquipments;

public record GetEquipmentsRequest(
    string? Type,
    bool? Unassigned
);
