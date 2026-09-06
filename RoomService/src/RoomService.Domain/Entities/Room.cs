using RoomService.Domain.Common;

namespace RoomService.Domain.Entities
{
    public class Room
    {
        private readonly List<RoomEquipment> _equipments = new();

        /// <summary>Limite da planta atual: um andar, ate 10 salas.</summary>
        public const int MaxPlanSlot = 10;

        public Guid Id {get; private set;}
        public string Name {get; private set;}
        public int Number {get; private set;}

        /// <summary>
        /// Qual poligono da planta fixa representa esta sala. Nulo = a sala nao
        /// aparece na planta, mas aparece nas listagens normalmente.
        /// Ver docs/product/information-architecture.md#tratamento-visual.
        /// </summary>
        public int? PlanSlot {get; private set;}

        public IReadOnlyCollection<RoomEquipment> Equipments => _equipments.AsReadOnly();

        protected Room() { }

        public Room(string name, int number)
        {
            Id = Guid.NewGuid();
            Rename(name);
            ChangeNumber(number);
        }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Informe o nome.");
            }

            if (name.Trim().Length < 3)
            {
                throw new DomainException("O nome precisa de ao menos 3 caracteres.");
            }

            Name = name.Trim();
        }

        public void ChangeNumber(int number)
        {
            if (number <= 0)
            {
                throw new DomainException("O número da sala precisa ser maior que 0.");
            }

            Number = number;
        }

        public void AssignPlanSlot(int? planSlot)
        {
            if (planSlot is not null && (planSlot < 1 || planSlot > MaxPlanSlot))
            {
                throw new DomainException($"A posição na planta precisa estar entre 1 e {MaxPlanSlot}.");
            }

            PlanSlot = planSlot;
        }

        public void AddEquipment(Guid equipmentId)
        {
            if (equipmentId == Guid.Empty)
            {
                throw new DomainException("Informe o equipamento.");
            }

            if(_equipments.Any(x => x.EquipmentId == equipmentId))
            {
                throw new DomainException("Este equipamento já está na sala.");
            }

            _equipments.Add(new RoomEquipment(Id, equipmentId));
        }

        public void RemoveEquipment(Guid equipmentId)
        {
            var association = _equipments.FirstOrDefault(x => x.EquipmentId == equipmentId);

            if (association is null)
            {
                throw new DomainException("Este equipamento não está na sala.");
            }

            _equipments.Remove(association);
        }
    }    
}