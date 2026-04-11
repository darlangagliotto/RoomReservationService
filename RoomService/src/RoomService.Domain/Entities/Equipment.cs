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
                throw new DomainException("Equipment type is required.");
            }

            if (type.Trim().Length < 3)
            {
                throw new DomainException("Equipment type must be at least 3 characters long.");
            }

            Type = type.Trim();
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

            SerialNumber = serialNumber;
        }

        public void ChangePurchaseDate(DateTime purchaseDate)
        {
            if (purchaseDate == default)
            {
                throw new DomainException("Purchase date is required.");
            }

            if (purchaseDate.Date > DateTime.UtcNow.Date)
            {
                throw new DomainException("Purchase date cannot be in the future.");
            }
            
            if (purchaseDate.Year < 1990)
            {
                throw new DomainException("Invalid purchase date for the business context.");
            }

            PurchaseDate = purchaseDate;
        }
    }
}