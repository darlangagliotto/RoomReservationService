namespace ReservationService.Application.UseCases.GetAvailability;

public record RoomAvailabilityResponse(
    Guid RoomId,
    string RoomName,
    int RoomNumber,
    string Status,
    /// Fim do bloco ocupado, considerando reservas encadeadas. Nulo se livre.
    DateTime? BusyUntil,
    /// Inicio da proxima reserva no mesmo dia. Nulo se nao houver.
    DateTime? NextReservationAt
);
