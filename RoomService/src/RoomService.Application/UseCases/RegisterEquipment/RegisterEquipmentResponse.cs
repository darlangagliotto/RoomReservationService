namespace RoomService.Application.UseCases.RegisterEquipment;

public record RegisterEquipmentResponse(
    Guid Id,
    string Type,
    string Brand,
    string SerialNumber,
    DateTime PurchaseDate
);