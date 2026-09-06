using RoomService.Domain.Entities;

namespace RoomService.Domain.Repositories
{
    /// <summary>Equipamento e a sala onde esta alocado (nulo = livre).</summary>
    public record EquipmentAllocation(
        Equipment Equipment,
        Guid? RoomId
    );
}
