namespace ReservationService.Application.UseCases.CreateReservation
{
    public record CreateReservationRequest(
        Guid UserId,
        Guid RoomId,
        DateTime StartDate,
        DateTime EndDate
    );
}
