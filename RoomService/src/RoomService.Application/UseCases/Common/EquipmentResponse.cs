namespace RoomService.Application.UseCases.Common;

public record EquipmentResponse(
    Guid Id,
    string Type,
    /// Ancora efetiva: a sobrescrita da sala quando houver, senao a do tipo.
    string Placement,
    string Brand,
    string SerialNumber,
    DateTime PurchaseDate,
    /// Sala onde esta alocado; nulo quando livre. Evita o cliente cruzar listas.
    Guid? RoomId
);
