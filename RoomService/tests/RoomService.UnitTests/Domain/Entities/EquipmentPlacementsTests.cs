using FluentAssertions;
using RoomService.Domain.Entities;
using Xunit;

namespace RoomService.UnitTests.Domain.Entities
{
    public class EquipmentPlacementsTests
    {
        [Theory]
        [InlineData(EquipmentType.Tv, EquipmentPlacement.Parede)]
        [InlineData(EquipmentType.Monitor, EquipmentPlacement.Parede)]
        [InlineData(EquipmentType.QuadroBranco, EquipmentPlacement.Parede)]
        [InlineData(EquipmentType.Projetor, EquipmentPlacement.Teto)]
        [InlineData(EquipmentType.ArCondicionado, EquipmentPlacement.Teto)]
        [InlineData(EquipmentType.Telefone, EquipmentPlacement.Mesa)]
        [InlineData(EquipmentType.Notebook, EquipmentPlacement.Mesa)]
        [InlineData(EquipmentType.Dock, EquipmentPlacement.Mesa)]
        [InlineData(EquipmentType.Flipchart, EquipmentPlacement.Piso)]
        [InlineData(EquipmentType.Cadeira, EquipmentPlacement.Piso)]
        [InlineData(EquipmentType.Outro, EquipmentPlacement.Piso)]
        public void Should_Anchor_Type_To_Its_Placement(EquipmentType type, EquipmentPlacement expected)
        {
            EquipmentPlacements.For(type).Should().Be(expected);
        }

        [Fact]
        public void Should_Anchor_Every_Known_Type()
        {
            // Tipo novo sem ancora cai no default e passa despercebido; este
            // teste garante que a tabela cobre o vocabulario inteiro.
            foreach (var type in Enum.GetValues<EquipmentType>())
            {
                Enum.IsDefined(EquipmentPlacements.For(type)).Should().BeTrue();
            }
        }
    }
}
