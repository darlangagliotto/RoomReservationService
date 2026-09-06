namespace RoomService.Application.UseCases.RegisterEquipment;

public record RegisterEquipmentRequest(
    /// Valor do vocabulario EquipmentType. Comparacao nao diferencia caixa.
    string Type,
    string Brand,
    string SerialNumber,
    DateTime PurchaseDate
);
