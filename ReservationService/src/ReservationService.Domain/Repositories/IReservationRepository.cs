using ReservationService.Domain.Entities;

namespace ReservationService.Domain.Repositories
{
    public interface IReservationRepository
    {
        Task<List<Reservation>> GetAllAsync();
        Task<Reservation> GetReservationById(Guid id);
        Task<List<Reservation>> GetByDate(DateTime startTime, DateTime endTime);
        Task<List<Reservation?>> GetByRoomId(Guid roomId);
        Task UpdateAsync(Reservation reservation);
        Task DeleteAsync(Reservation reservation);        
        IQueryable<Reservation> Query();
    }
}