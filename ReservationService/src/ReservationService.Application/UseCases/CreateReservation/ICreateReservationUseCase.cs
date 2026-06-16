using ReservationService.Domain.Common;

namespace ReservationService.Application.UseCases.CreateReservation
{
    public interface ICreateReservationUseCase
    {
        Task<Result<CreateReservationResponse>> ExecuteAsync(CreateReservationRequest request);
    }
}
