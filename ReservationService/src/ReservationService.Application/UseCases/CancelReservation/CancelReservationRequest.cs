namespace ReservationService.Application.UseCases.CancelReservation
{
    public record CancelReservationRequest(
        Guid ReservationId
    );
}
