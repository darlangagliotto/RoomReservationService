using ReservationService.Application.UseCases.Common;
using ReservationService.Domain.Common;

namespace ReservationService.Application.UseCases.GetReservations
{
    public interface IGetReservationsUseCase
    {
        Task<Result<List<ReservationResponse>>> ExecuteAsync(GetReservationsRequest request);
    }
}