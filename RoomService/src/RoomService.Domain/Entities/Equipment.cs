using RoomService.Domain.Common;

namespace RoomService.Domain.Entities
{
    public class Equipment
    {
        public Guid Id {get; private set;}
        public string Type {get; private set;}
        public string Brand {get; private set;}
        public string SerialNumber {get; private set;}
        public DateTime PurchaseDate {get; private set;}

        public Equipment(string type, string brand, string serialNumber, DateTime purchaseDate)
        {
            Id = Guid.NewGuid();
            ChangeType(type);
            ChangeBrand(brand);
            ChangeSerialNumber(serialNumber);
            ChangePurchaseDate(purchaseDate);
        }

        protected Equipment() { }

        public void ChangeType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new DomainException("Tipo do equipamento é obrigatório.");
            }

            if (type.Trim().Length < 3)
            {
                throw new DomainException("Tipo do equipamento deve ter no mínimo 3 caracteres.");
            }

            Type = type.Trim();
        }

        public void ChangeBrand(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
            {
                throw new DomainException("A marca é obrigatória.");
            }

            if (brand.Trim().Length < 3)
            {
                throw new DomainException("A marca deve ter no mínimo 3 caracteres.");
            }

            Brand = brand.Trim();
        }

        public void ChangeSerialNumber(string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new DomainException("Serial number é obrigatório.");
            }

            if (serialNumber.Trim().Length < 3)
            {
                throw new DomainException("Serial number deve ter no mínimo 3 caracteres.");
            }

            SerialNumber = serialNumber;
        }

        public void ChangePurchaseDate(DateTime purchaseDate)
        {
            if (purchaseDate == default)
            {
                throw new DomainException("Data de compra é obrigatória.");
            }

            if (purchaseDate.Date > DateTime.UtcNow.Date)
            {
                throw new DomainException("Data de compra não pode ser futura.");
            }
            
            if (purchaseDate.Year < 1990)
            {
                throw new DomainException("Data de compra inválida para o contexto do negócio.");
            }

            PurchaseDate = purchaseDate;
        }
    }
}