using RoomService.Domain.Entities;

namespace RoomService.Domain.Repositories
{
    public interface IEquipmentRepository
    {
        Task<Equipment?> GetByIdAsync(Guid id);
        Task<Equipment?> GetByTypeAsync(EquipmentType equipmentType);
        Task<Equipment?> GetByBrandAsync(string brand);
        Task<Equipment?> GetBySerialNumberAsync(string serialNumber);

        /// <summary>
        /// Listagem com a sala de cada equipamento. Uma consulta so — nao uma
        /// por equipamento. Ver docs/specs/004.
        /// </summary>
        Task<List<EquipmentAllocation>> GetAllAsync(EquipmentType? type, bool unassignedOnly);

        Task AddSync(Equipment equipment);
        Task UpdateAsync(Equipment equipment);
        Task DeleteAsync(Equipment equipment);
    }
}
