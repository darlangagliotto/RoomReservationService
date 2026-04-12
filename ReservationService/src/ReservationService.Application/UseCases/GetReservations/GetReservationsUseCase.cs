using Microsoft.EntityFrameworkCore;
using ReservationService.Domain.Common;
using ReservationService.Domain.Entities;
using ReservationService.Domain.Repositories;
using ReservationService.Application.UseCases.Common;
using ReservationService.Application.Services;

namespace ReservationService.Application.UseCases.GetReservations
{
    public class GetReservationsUseCase : IGetReservationsUseCase
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IRoomServiceClient _roomServiceClient;
        private readonly IUserServiceClient _userServiceClient;

        public GetReservationsUseCase(
            IReservationRepository reservationRepository, 
            IRoomServiceClient roomServiceClient,
            IUserServiceClient userServiceClient)
        {
            _reservationRepository = reservationRepository;
            _roomServiceClient = roomServiceClient;
            _userServiceClient = userServiceClient;
        }

        public async Task<Result<List<ReservationResponse>>> ExecuteAsync(GetReservationsRequest request)
        {
            var reservationsResponse = await FindReservationsAsync(request);

            if (reservationsResponse.Count == 0)
            {
                return Result<List<ReservationResponse>>.Failure("No reservations found.");
            }

            return Result<List<ReservationResponse>>.Success(reservationsResponse);
        }

        private async Task<List<ReservationResponse>> FindReservationsAsync(GetReservationsRequest request)
        {
            var query = _reservationRepository.Query();

            var user = await ResolveUserAsync(request.UserId);
            if (user != null)
            {
                query = query.Where(r => r.UserId == user.Id);
            }

            var room = await ResolveRoomAsync(request);
            if (room != null)
            {
                query = query.Where(r => r.RoomId == room.Id);
            }
            else if (request.RoomId.HasValue || request.RoomNumber.HasValue || !string.IsNullOrWhiteSpace(request.RoomName))
            {
                return new List<ReservationResponse>();
            }

            if (request.StartDate.HasValue)
                query = query.Where(r => r.StartTime >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(r => r.EndTime <= request.EndDate.Value);

            var reservations = await query.ToListAsync();
            var responses = new List<ReservationResponse>();
            foreach (var reservation in reservations)
            {
                // Buscar nome do usuário
                var userResp = await _userServiceClient.GetUserByIdAsync(reservation.UserId);
                var userName = userResp?.Name ?? string.Empty;

                // Buscar dados da sala
                var roomResp = await _roomServiceClient.GetRoomByIdAsync(reservation.RoomId);
                var roomName = roomResp?.Name ?? string.Empty;
                var roomNumber = roomResp?.Number ?? 0;

                responses.Add(new ReservationResponse(
                    reservation.Id,
                    reservation.UserId,
                    userName,
                    reservation.RoomId,
                    roomName,
                    roomNumber,
                    reservation.StartTime,
                    reservation.EndTime
                ));
            }
            return responses;
        }

        private async Task<GetUserResponse?> ResolveUserAsync(Guid? userId)
        {
            if (!userId.HasValue)
            {
                return null;
            }
            return await _userServiceClient.GetUserByIdAsync(userId.Value);
        }

        private async Task<GetRoomResponse?> ResolveRoomAsync(GetReservationsRequest request)
        {
            GetRoomResponse? roomById = null;
            if (request.RoomId.HasValue)
                roomById = await _roomServiceClient.GetRoomByIdAsync(request.RoomId.Value);

            GetRoomResponse? roomByNumber = null;
            if (request.RoomNumber.HasValue)
                roomByNumber = await _roomServiceClient.GetRoomByNumberAsync(request.RoomNumber.Value);

            GetRoomResponse? roomByName = null;
            if (!string.IsNullOrWhiteSpace(request.RoomName))
                roomByName = await _roomServiceClient.GetRoomByNameAsync(request.RoomName);

            // Cria lista com todas as salas encontradas (ignorando nulos)
            var rooms = new[] { roomById, roomByNumber, roomByName }.Where(r => r != null).ToList();

            // Se mais de um identificador foi informado, todos devem apontar para a mesma sala
            if (rooms.Count > 1 && rooms.Select(r => r!.Id).Distinct().Count() > 1)
                return null; // Inconsistência: múltiplos identificadores diferentes

            // Retorna a sala resolvida (ou null se nenhum identificador foi informado)
            return rooms.FirstOrDefault();
        }
    }
}