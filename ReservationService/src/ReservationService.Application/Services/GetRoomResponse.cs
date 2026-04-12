namespace ReservationService.Application.Services
{
    public record GetUserResponse(
        Guid Id,
        string Name        
    );
}