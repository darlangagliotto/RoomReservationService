using ReservationService.Application.Services;
using ReservationService.Domain.Common;
using ReservationService.Domain.Entities;
using ReservationService.Domain.Repositories;

namespace ReservationService.Application.UseCases.GetAvailability
{
    public class GetAvailabilityUseCase : IGetAvailabilityUseCase
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly IRoomServiceClient _roomServiceClient;

        public GetAvailabilityUseCase(
            IReservationRepository reservationRepository,
            IRoomServiceClient roomServiceClient)
        {
            _reservationRepository = reservationRepository;
            _roomServiceClient = roomServiceClient;
        }

        public async Task<Result<List<RoomAvailabilityResponse>>> ExecuteAsync(GetAvailabilityRequest request)
        {
            if (request.Start is null || request.End is null)
            {
                return Result<List<RoomAvailabilityResponse>>.Failure("Start and end are required.");
            }

            var start = ToUtc(request.Start.Value);
            var end = ToUtc(request.End.Value);

            if (start >= end)
            {
                return Result<List<RoomAvailabilityResponse>>.Failure("Start must be before end.");
            }

            List<GetRoomResponse> rooms;
            try
            {
                rooms = await _roomServiceClient.GetAllRoomsAsync();
            }
            catch (Exception)
            {
                // Planta com salas faltando e pior que planta que nao carrega:
                // a falha e total e explicita. Ver docs/specs/003.
                return Result<List<RoomAvailabilityResponse>>.Failure("Rooms are unavailable.");
            }

            // Uma consulta so, cobrindo o intervalo e o resto do dia de `end`
            // — o recorte que "Reservada" precisa enxergar.
            var dayEnd = end.Date.AddDays(1);
            var reservations = await _reservationRepository.GetOverlappingOrLaterSameDayAsync(start, dayEnd);

            var byRoom = reservations
                .GroupBy(r => r.RoomId)
                .ToDictionary(g => g.Key, g => g.OrderBy(r => r.StartTime).ToList());

            var responses = rooms
                .Select(room => Evaluate(room, byRoom.GetValueOrDefault(room.Id, []), start, end, dayEnd))
                .ToList();

            return Result<List<RoomAvailabilityResponse>>.Success(responses);
        }

        private static RoomAvailabilityResponse Evaluate(
            GetRoomResponse room,
            List<Reservation> reservations,
            DateTime start,
            DateTime end,
            DateTime dayEnd)
        {
            // Mesma regra de sobreposicao da criacao de reserva: intervalos que
            // so se tocam nas bordas nao sobrepoem.
            var overlapping = reservations
                .Where(r => start < r.EndTime && end > r.StartTime)
                .OrderBy(r => r.StartTime)
                .ToList();

            if (overlapping.Count > 0)
            {
                return new RoomAvailabilityResponse(
                    room.Id, room.Name, room.Number,
                    RoomAvailabilityStatus.EmUso.ToString(),
                    BusyUntil(reservations, overlapping[0].EndTime),
                    null);
            }

            var next = reservations
                .Where(r => r.StartTime >= end && r.StartTime < dayEnd)
                .OrderBy(r => r.StartTime)
                .FirstOrDefault();

            return next is null
                ? new RoomAvailabilityResponse(room.Id, room.Name, room.Number,
                    RoomAvailabilityStatus.Disponivel.ToString(), null, null)
                : new RoomAvailabilityResponse(room.Id, room.Name, room.Number,
                    RoomAvailabilityStatus.Reservada.ToString(), null, next.StartTime);
        }

        /// <summary>
        /// Reservas encadeadas contam como um bloco so: 14-15 seguida de 15-16
        /// devolve 16, e nao 15 — senao a UI promete uma sala que continua ocupada.
        /// </summary>
        private static DateTime BusyUntil(List<Reservation> reservations, DateTime blockEnd)
        {
            bool extended;
            do
            {
                extended = false;
                foreach (var reservation in reservations)
                {
                    if (reservation.StartTime <= blockEnd && reservation.EndTime > blockEnd)
                    {
                        blockEnd = reservation.EndTime;
                        extended = true;
                    }
                }
            } while (extended);

            return blockEnd;
        }

        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
