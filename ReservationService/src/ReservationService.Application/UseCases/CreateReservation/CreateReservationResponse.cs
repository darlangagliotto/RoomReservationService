using ReservationService.Application.UseCases.Common;

namespace ReservationService.Application.UseCases.CreateReservation;

public record CreateReservationResponse(
    ReservationResponse Reservation
);
