using RoomService.Application.UseCases.Common;
using RoomService.Domain.Entities;

namespace RoomService.Application.UseCases.Common.Services
{
    public interface IEquipmentResponseMapper
    {
         Task<List<EquipmentResponse>> MapEquipmentsAsync(IEnumerable<RoomEquipment> roomEquipments);
    }
}