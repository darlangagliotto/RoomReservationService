using FluentAssertions;
using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using Xunit;

namespace RoomService.UnitTests.Domain.Entities
{
    public class EquipmentTests
    {
        private static Equipment CreateValidEquipment()
            => new(EquipmentType.Projetor, "Epson", "SN-001", new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc));

        [Fact]
        public void Should_Derive_Placement_From_Type()
        {
            CreateValidEquipment().Placement.Should().Be(EquipmentPlacement.Teto);
        }

        [Fact]
        public void Should_Reject_Type_Outside_The_Vocabulary()
        {
            var act = () => new Equipment((EquipmentType)999, "Epson", "SN-001", DateTime.UtcNow.Date);

            act.Should().Throw<DomainException>().WithMessage("Unknown equipment type.");
        }

        [Fact]
        public void Should_Normalize_Unspecified_Purchase_Date_To_Utc()
        {
            // A coluna e timestamptz: sem esta normalizacao o insert quebra com 500.
            var semFuso = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Unspecified);

            var equipment = new Equipment(EquipmentType.Tv, "Samsung", "SN-002", semFuso);

            equipment.PurchaseDate.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void Should_Reject_Purchase_Date_In_The_Future()
        {
            var act = () => new Equipment(EquipmentType.Tv, "Samsung", "SN-002", DateTime.UtcNow.AddDays(1));

            act.Should().Throw<DomainException>().WithMessage("Purchase date cannot be in the future.");
        }

        [Fact]
        public void Should_Reject_Purchase_Date_Before_The_Business_Context()
        {
            var act = () => new Equipment(EquipmentType.Tv, "Samsung", "SN-002", new DateTime(1989, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            act.Should().Throw<DomainException>().WithMessage("Invalid purchase date for the business context.");
        }
    }
}
