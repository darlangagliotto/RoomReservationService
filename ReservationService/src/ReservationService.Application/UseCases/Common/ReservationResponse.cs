namespace ReservationService.Application.UseCases.Common
{
    public record ReservationResponse(
        Guid Id,
        Guid UserId,
        string UserName,
        Guid RoomId,
        string RoomName,
        int RoomNumber,
        DateTime StartDate,
        DateTime EndDate
    );
}
