using FluentAssertions;
using Moq;
using RoomService.Application.UseCases.Common.Services;
using RoomService.Application.UseCases.RemoveEquipmentFromRoom;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;
using Xunit;

namespace RoomService.UnitTests.Application.RemoveEquipmentFromRoom
{
    public class RemoveEquipmentFromRoomUseCaseTests
    {
        private readonly Mock<IRoomRepository> _roomRepository = new();
        private readonly Mock<IEquipmentResponseMapper> _equipmentResponseMapper = new();

        private static Room CreateValidRoom() => new("Sala Azul", 101);

        private RemoveEquipmentFromRoomUseCase CreateUseCase()
        {
            _equipmentResponseMapper
                .Setup(m => m.MapEquipmentsAsync(It.IsAny<IEnumerable<RoomEquipment>>()))
                .ReturnsAsync([]);

            return new RemoveEquipmentFromRoomUseCase(_roomRepository.Object, _equipmentResponseMapper.Object);
        }

        [Fact]
        public async Task Should_Fail_When_Room_Does_Not_Exist()
        {
            _roomRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Room?)null);
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new RemoveEquipmentFromRoomRequest(Guid.NewGuid(), Guid.NewGuid()));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Sala não encontrada.");
        }

        [Fact]
        public async Task Should_Fail_When_Equipment_Is_Not_In_The_Room()
        {
            var room = CreateValidRoom();
            _roomRepository.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new RemoveEquipmentFromRoomRequest(room.Id, Guid.NewGuid()));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Este equipamento não está na sala.");
            _roomRepository.Verify(r => r.UpdateAsync(It.IsAny<Room>()), Times.Never);
        }

        [Fact]
        public async Task Should_Remove_Allocated_Equipment_And_Persist()
        {
            var room = CreateValidRoom();
            var equipmentId = Guid.NewGuid();
            room.AddEquipment(equipmentId);
            _roomRepository.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new RemoveEquipmentFromRoomRequest(room.Id, equipmentId));

            result.IsSuccess.Should().BeTrue();
            room.Equipments.Should().BeEmpty();
            _roomRepository.Verify(r => r.UpdateAsync(room), Times.Once);
        }
    }
}
