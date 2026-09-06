using ReservationService.Domain.Common;

namespace ReservationService.Application.UseCases.GetAvailability
{
    public interface IGetAvailabilityUseCase
    {
        Task<Result<List<RoomAvailabilityResponse>>> ExecuteAsync(GetAvailabilityRequest request);
    }
}
