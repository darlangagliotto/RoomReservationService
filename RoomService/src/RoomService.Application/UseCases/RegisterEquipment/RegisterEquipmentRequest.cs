namespace RoomService.Application.UseCases.RegisterEquipment;

public record RegisterEquipmentRequest(
    string Type,
    string Brand,
    string SerialNumber,
    DateTime PurchaseDate
);