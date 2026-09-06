using RoomService.Domain.Common;

namespace RoomService.Domain.Entities
{
    public class RoomEquipment
    {
        public Guid RoomId {get; private set;}
        public Guid EquipmentId {get; private set;}

        /// <summary>
        /// Sobrescrita da ancora. Nulo significa "use a ancora do tipo" — e o
        /// caso de praticamente todo cadastro. Existe para precisao virar
        /// aditiva no futuro sem migration dolorosa. Ver docs/specs/004.
        /// </summary>
        public EquipmentPlacement? Placement {get; private set;}

        internal RoomEquipment(Guid roomId, Guid equipmentId)
        {
            if (roomId == Guid.Empty)
            {
                throw new DomainException("Informe a sala.");
            }

            if (equipmentId == Guid.Empty)
            {
                throw new DomainException("Informe o equipamento.");
            }
            
            RoomId = roomId;
            EquipmentId = equipmentId;
        }

        protected RoomEquipment() { }

        public void OverridePlacement(EquipmentPlacement? placement) => Placement = placement;
    }
}
