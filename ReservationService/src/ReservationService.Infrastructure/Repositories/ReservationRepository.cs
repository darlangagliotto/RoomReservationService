using Microsoft.EntityFrameworkCore;
using ReservationService.Domain.Entities;
using ReservationService.Domain.Repositories;
using ReservationService.Infrastructure.Data;

namespace ReservationService.Infrastructure.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
            public IQueryable<Reservation> Query()
            {
                return _context.Reservations.AsQueryable();
            }
    
        private readonly ReservationDbContext _context;

        public ReservationRepository(ReservationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Reservation>> GetAllAsync()
        {
            return await _context.Reservations
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Reservation> GetReservationById(Guid id)
        {
            return await _context.Reservations.FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task<List<Reservation>> GetByDate(DateTime startTime, DateTime endTime)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Where(r => r.StartTime >= startTime && r.EndTime <= endTime)
                .ToListAsync();
        }

        public async Task<List<Reservation?>> GetByRoomId(Guid roomId)
        {
            return await _context.Reservations
                .AsNoTracking()
                .Where(r => r.RoomId == roomId)
                .ToListAsync();
        }

        public async Task AddAsync(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Reservation reservation)
        {
            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Reservation reservation)
        {
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
        }
    }   
}