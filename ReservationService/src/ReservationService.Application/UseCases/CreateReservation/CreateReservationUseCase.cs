using ReservationService.Domain.Common;
using ReservationService.Domain.Entities;
using ReservationService.Domain.Repositories;
using ReservationService.Application.UseCases.Common;
using ReservationService.Application.Services;

namespace ReservationService.Application.UseCases.CreateReservation
{
    public class CreateReservationUseCase : ICreateReservationUseCase
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IRoomServiceClient _roomServiceClient;
        private readonly IUserServiceClient _userServiceClient;

        public CreateReservationUseCase(
            IReservationRepository reservationRepository,
            IRoomServiceClient roomServiceClient,
            IUserServiceClient userServiceClient)
        {
            _reservationRepository = reservationRepository;
            _roomServiceClient = roomServiceClient;
            _userServiceClient = userServiceClient;
        }

        public async Task<Result<CreateReservationResponse>> ExecuteAsync(CreateReservationRequest request)
        {
            var user = await _userServiceClient.GetUserByIdAsync(request.UserId);
            if (user is null)
            {
                return Result<CreateReservationResponse>.Failure("User not found.");
            }

            var room = await _roomServiceClient.GetRoomByIdAsync(request.RoomId);
            if (room is null)
            {
                return Result<CreateReservationResponse>.Failure("Room not found.");
            }

            var availabilityValidation = await ValidateRoomAvailabilityAsync(request);
            if (!availabilityValidation.IsSuccess)
            {
                return Result<CreateReservationResponse>.Failure(availabilityValidation.Error!);
            }

            Reservation reservation;
            try
            {
                reservation = new Reservation(
                    request.UserId,
                    request.RoomId,
                    request.StartDate,
                    request.EndDate
                );
            }
            catch (DomainException ex)
            {
                return Result<CreateReservationResponse>.Failure(ex.Message);
            }

            await _reservationRepository.AddAsync(reservation);

            return Result<CreateReservationResponse>.Success(
                new CreateReservationResponse(
                    new ReservationResponse(
                        reservation.Id,
                        reservation.UserId,
                        user.Name,
                        reservation.RoomId,
                        room.Name,
                        room.Number,
                        reservation.StartTime,
                        reservation.EndTime
                    )
                )
            );
        }

        private async Task<Result<bool>> ValidateRoomAvailabilityAsync(CreateReservationRequest request)
        {
            var roomReservations = await _reservationRepository.GetByRoomId(request.RoomId);

            var hasOverlap = roomReservations
                .Where(r => r is not null)
                .Any(r => request.StartDate < r!.EndTime && request.EndDate > r.StartTime);

            if (hasOverlap)
            {
                return Result<bool>.Failure("Room is already reserved for the requested period.");
            }

            return Result<bool>.Success(true);
        }
    }
}
