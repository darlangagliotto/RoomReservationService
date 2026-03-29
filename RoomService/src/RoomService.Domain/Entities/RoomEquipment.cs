using RoomService.Domain.Common;

namespace RoomService.Domain.Entities
{
    public class RoomEquipment
    {
        public Guid RoomId {get; private set;}
        public Guid EquipmentId {get; private set;}

        internal RoomEquipment(Guid roomId, Guid equipmentId)
        {
            if (roomId == Guid.Empty)
            {
                throw new DomainException("Sala é obrigatória.");
            }

            if (equipmentId == Guid.Empty)
            {
                throw new DomainException("Equipamento é obrigatório.");
            }
            
            RoomId = roomId;
            EquipmentId = equipmentId;
        }

        protected RoomEquipment() { }
    }
}