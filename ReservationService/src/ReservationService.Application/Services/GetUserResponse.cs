namespace ReservationService.Application.Services
{
    public record GetRoomResponse(
        Guid Id,
        string Name,
        int Number
    );
}