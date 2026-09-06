namespace RoomService.Application.UseCases.RegisterRoom;

public record RegisterRoomRequest(
    string Name,
    int Number,
    List<Guid> EquipmentIds,
    /// Poligono da planta fixa. Nulo = sala nao aparece na planta.
    int? PlanSlot
);
