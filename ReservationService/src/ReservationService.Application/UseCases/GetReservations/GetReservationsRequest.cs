namespace ReservationService.Application.UseCases.GetReservations
{
    public record GetReservationsRequest(
        Guid? UserId,
        string? UserName,        
        Guid? RoomId,
        int? RoomNumber,
        string? RoomName,
        DateTime? StartDate,
        DateTime? EndDate
    );
}