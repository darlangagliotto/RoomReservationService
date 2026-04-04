namespace RoomService.Application.UseCases.Common;

public record EquipmentResponse(
    Guid Id,
    string Type,
    string Brand,
    string SerialNumber,
    DateTime PurchaseDate
);