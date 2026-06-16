using ReservationService.Domain.Common;

namespace ReservationService.Application.UseCases.CancelReservation
{
    public interface ICancelReservationUseCase
    {
        Task<Result<CancelReservationResponse>> ExecuteAsync(CancelReservationRequest request);
    }
}
