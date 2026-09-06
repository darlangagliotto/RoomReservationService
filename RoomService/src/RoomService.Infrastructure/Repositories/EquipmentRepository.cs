using Microsoft.EntityFrameworkCore;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;
using RoomService.Infrastructure.Data;

namespace RoomService.Infrastructure.Repositories
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly RoomDbContext _context;

        public EquipmentRepository(RoomDbContext context)
        {
            _context = context;
        }

        public async Task<Equipment?> GetByTypeAsync(EquipmentType equipmentType)
        {
            return await _context.Equipments.FirstOrDefaultAsync(u => u.Type == equipmentType);
        }

        public async Task<Equipment?> GetByIdAsync(Guid id)
        {
            return await _context.Equipments.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Equipment?> GetByBrandAsync(string brand)
        {
            return await _context.Equipments.FirstOrDefaultAsync(u => u.Brand == brand);
        }

        public async Task<Equipment?> GetBySerialNumberAsync(string serialNumber)
        {
            return await _context.Equipments.FirstOrDefaultAsync(u => u.SerialNumber == serialNumber);
        }

        public async Task<List<EquipmentAllocation>> GetAllAsync(EquipmentType? type, bool unassignedOnly)
        {
            // Left join contra RoomEquipment resolve alocacao e filtro numa
            // consulta so, sem N+1.
            var query =
                from equipment in _context.Equipments.AsNoTracking()
                join link in _context.Set<RoomEquipment>().AsNoTracking()
                    on equipment.Id equals link.EquipmentId into links
                from link in links.DefaultIfEmpty()
                select new { equipment, RoomId = (Guid?)link.RoomId };

            if (type is not null)
            {
                query = query.Where(x => x.equipment.Type == type);
            }

            if (unassignedOnly)
            {
                query = query.Where(x => x.RoomId == null);
            }

            var rows = await query
                .OrderBy(x => x.equipment.Brand)
                .ToListAsync();

            return rows
                .Select(x => new EquipmentAllocation(x.equipment, x.RoomId))
                .ToList();
        }

        public async Task AddSync(Equipment equipment)
        {
            await _context.Equipments.AddAsync(equipment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Equipment equipment)
        {
            _context.Equipments.Update(equipment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Equipment equipment)
        {
            _context.Equipments.Remove(equipment);
            await _context.SaveChangesAsync();
        }
    }
}
