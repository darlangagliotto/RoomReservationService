using RoomService.Application.UseCases.Common;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;

namespace RoomService.Application.UseCases.Common.Services
{
    public class EquipmentResponseMapper : IEquipmentResponseMapper
    {
        private readonly IEquipmentRepository _equipmentRepository;

        public EquipmentResponseMapper(IEquipmentRepository equipmentRepository)
        {
            _equipmentRepository = equipmentRepository;
        }
        public async Task<List<EquipmentResponse>> MapEquipmentsAsync(IEnumerable<RoomEquipment> roomEquipments)
        {
            var equipmentResponses = new List<EquipmentResponse>();

            foreach (var roomEquipment in roomEquipments)
            {
                var equipment = await _equipmentRepository.GetByIdAsync(roomEquipment.EquipmentId);
                if (equipment is null) continue;

                equipmentResponses.Add(new EquipmentResponse(
                    equipment.Id,
                    equipment.Type,
                    equipment.Brand,
                    equipment.SerialNumber,
                    equipment.PurchaseDate
                ));
            }
            return equipmentResponses;
        }
    }
}