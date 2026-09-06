using ReservationService.Domain.Entities;

namespace ReservationService.Domain.Repositories
{
    public interface IReservationRepository
    {
        Task<List<Reservation>> GetAllAsync();
        Task<Reservation?> GetReservationById(Guid id);
        Task<List<Reservation>> GetByDate(DateTime startTime, DateTime endTime);
        Task<List<Reservation?>> GetByRoomId(Guid roomId);

        /// <summary>Reservas que sobrepoem o intervalo ou comecam depois dele, ate o fim do dia.</summary>
        Task<List<Reservation>> GetOverlappingOrLaterSameDayAsync(DateTime start, DateTime dayEnd);
        Task AddAsync(Reservation reservation);
        Task UpdateAsync(Reservation reservation);
        Task DeleteAsync(Reservation reservation);
        IQueryable<Reservation> Query();
    }
}