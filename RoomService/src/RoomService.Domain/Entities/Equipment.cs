using RoomService.Domain.Common;

namespace RoomService.Domain.Entities
{
    public class Equipment
    {
        public Guid Id {get; private set;}
        public EquipmentType Type {get; private set;}
        public string Brand {get; private set;}
        public string SerialNumber {get; private set;}
        public DateTime PurchaseDate {get; private set;}

        /// <summary>Ancora padrao, derivada do tipo. Nao e coluna.</summary>
        public EquipmentPlacement Placement => EquipmentPlacements.For(Type);

        public Equipment(EquipmentType type, string brand, string serialNumber, DateTime purchaseDate)
        {
            Id = Guid.NewGuid();
            ChangeType(type);
            ChangeBrand(brand);
            ChangeSerialNumber(serialNumber);
            ChangePurchaseDate(purchaseDate);
        }

        protected Equipment() { }

        public void ChangeType(EquipmentType type)
        {
            if (!Enum.IsDefined(type))
            {
                throw new DomainException("Tipo de equipamento desconhecido.");
            }

            Type = type;
        }

        public void ChangeBrand(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
            {
                throw new DomainException("Informe a marca.");
            }

            if (brand.Trim().Length < 3)
            {
                throw new DomainException("A marca precisa de ao menos 3 caracteres.");
            }

            Brand = brand.Trim();
        }

        public void ChangeSerialNumber(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new DomainException("Informe o número de série.");
            }

            if (serialNumber.Trim().Length < 3)
            {
                throw new DomainException("O número de série precisa de ao menos 3 caracteres.");
            }

            SerialNumber = serialNumber.Trim();
        }

        public void ChangePurchaseDate(DateTime purchaseDate)
        {
            if (purchaseDate == default)
            {
                throw new DomainException("Informe a data de compra.");
            }

            // A coluna e timestamptz e o Npgsql so aceita Kind=Utc: uma data sem
            // fuso ("2024-01-10") chegava como Unspecified e derrubava o insert
            // com 500. Normalizar aqui e coerente com o Email, que tambem
            // normaliza na entrada. Data de compra nao tem semantica de hora.
            var normalized = purchaseDate.Kind switch
            {
                DateTimeKind.Utc => purchaseDate,
                DateTimeKind.Local => purchaseDate.ToUniversalTime(),
                _ => DateTime.SpecifyKind(purchaseDate, DateTimeKind.Utc)
            };

            if (normalized.Date > DateTime.UtcNow.Date)
            {
                throw new DomainException("A data de compra não pode ser futura.");
            }

            if (normalized.Year < 1990)
            {
                throw new DomainException("Data de compra fora do período aceito.");
            }

            PurchaseDate = normalized;
        }
    }
}
