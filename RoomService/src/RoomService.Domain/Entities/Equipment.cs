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
                throw new DomainException("Unknown equipment type.");
            }

            Type = type;
        }

        public void ChangeBrand(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
            {
                throw new DomainException("Brand is required.");
            }

            if (brand.Trim().Length < 3)
            {
                throw new DomainException("Brand must be at least 3 characters long.");
            }

            Brand = brand.Trim();
        }

        public void ChangeSerialNumber(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new DomainException("Serial number is required.");
            }

            if (serialNumber.Trim().Length < 3)
            {
                throw new DomainException("Serial number must be at least 3 characters long.");
            }

            SerialNumber = serialNumber.Trim();
        }

        public void ChangePurchaseDate(DateTime purchaseDate)
        {
            if (purchaseDate == default)
            {
                throw new DomainException("Purchase date is required.");
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
                throw new DomainException("Purchase date cannot be in the future.");
            }

            if (normalized.Year < 1990)
            {
                throw new DomainException("Invalid purchase date for the business context.");
            }

            PurchaseDate = normalized;
        }
    }
}
