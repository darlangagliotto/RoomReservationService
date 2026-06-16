using ReservationService.Domain.Common;
using ReservationService.Domain.Repositories;

namespace ReservationService.Application.UseCases.CancelReservation
{
    public class CancelReservationUseCase : ICancelReservationUseCase
    {
        private readonly IReservationRepository _reservationRepository;

        public CancelReservationUseCase(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public async Task<Result<CancelReservationResponse>> ExecuteAsync(CancelReservationRequest request)
        {
            var reservation = await _reservationRepository.GetReservationById(request.ReservationId);

            if (reservation is null)
            {
                return Result<CancelReservationResponse>.Failure("Reservation not found.");
            }

            if (reservation.StartTime <= DateTime.UtcNow)
            {
                return Result<CancelReservationResponse>.Failure("Cannot cancel a reservation that has already started.");
            }

            await _reservationRepository.DeleteAsync(reservation);

            return Result<CancelReservationResponse>.Success(
                new CancelReservationResponse(reservation.Id)
            );
        }
    }
}
