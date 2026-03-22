namespace RoomService.Domain.Entities
{
    public class Room
    {
        private readonly List<RoomEquipment> _equipments = new();

        public Guid Id {get; private set;}
        public string Name {get; private set;}
        public int Number {get; private set;}

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
                throw new DomainException("Nome é obrigatório.");
            }

            if (name.Trim().Length < 3)
            {
                throw new DomainException("Nome deve ter no mínimo 3 caracteres.");
            }

            Name = name.Trim();
        }

        public void ChangeNumber(int number)
        {
            if (number <= 0)
            {
                throw new DomainException("Numero da sala deve ser maior do que 0.");
            }

            Number = number;
        }

        public void AddEquipment(Guid equipmentId)
        {
            if (equipmentId == Guid.Empty)
            {
                throw new DomainException("Equipamento é obrigatório.");
            }

            if(_equipments.Any(x => x.EquipmentId == equipmentId))
            {
                throw new DomainException("Equipamento já está associado à sala.");
            }

            _equipments.Add(new RoomEquipment(Id, equipmentId));
        }

        public void RemoveEquipment(Guid equipmentId)
        {
            var association = _equipments.FirstOrDefault(x => x.EquipmentId == equipmentId);

            if (association is null)
            {
                throw new DomainException("Equipamento não está associado à sala.");
            }

            _equipments.Remove(association);
        }
    }    
}