using FluentAssertions;
using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using Xunit;

namespace RoomService.UnitTests.Domain.Entities
{
    public class RoomPlanSlotTests
    {
        private static Room CreateValidRoom() => new("Sala Azul", 101);

        [Fact]
        public void Should_Start_Without_Plan_Slot()
        {
            CreateValidRoom().PlanSlot.Should().BeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void Should_Accept_Slot_Inside_The_Plan(int slot)
        {
            var room = CreateValidRoom();

            room.AssignPlanSlot(slot);

            room.PlanSlot.Should().Be(slot);
        }

        [Fact]
        public void Should_Accept_Null_To_Remove_The_Room_From_The_Plan()
        {
            var room = CreateValidRoom();
            room.AssignPlanSlot(4);

            room.AssignPlanSlot(null);

            room.PlanSlot.Should().BeNull();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        [InlineData(-1)]
        public void Should_Reject_Slot_Outside_The_Plan(int slot)
        {
            var act = () => CreateValidRoom().AssignPlanSlot(slot);

            act.Should().Throw<DomainException>()
               .WithMessage("A posição na planta precisa estar entre 1 e 10.");
        }
    }
}
