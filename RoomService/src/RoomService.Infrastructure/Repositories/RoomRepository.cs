using Microsoft.EntityFrameworkCore;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;
using RoomService.Infrastructure.Data;

namespace RoomService.Infrastructure.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly RoomDbContext _context;

        public RoomRepository(RoomDbContext context)
        {
            _context = context;
        }

        public async Task<Room?> GetByNameAsync(string name)
        {
            return await _context.Rooms.FirstOrDefaultAsync(u => u.Name == name);
        }

        public async Task<Room?> GetByIdAsync(Guid id)
        {
            return await _context.Rooms.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Room?> GetByNumberAsync(int roomNumber)
        {
            return await _context.Rooms.FirstOrDefaultAsync(u => u.Number == roomNumber);
        }

        public async Task<Room?> GetByNameAndNumberAsync(string name, int roomNumber)
        {
            return await _context.Rooms.FirstOrDefaultAsync(u => u.Name == name && u.Number == roomNumber);
        }

        public async Task AddSync(Room room)
        {
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Room room)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Room room)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
    }
}
