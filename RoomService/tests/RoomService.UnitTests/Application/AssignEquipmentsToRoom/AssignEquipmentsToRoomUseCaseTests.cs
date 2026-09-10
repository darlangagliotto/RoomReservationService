using FluentAssertions;
using Moq;
using RoomService.Application.UseCases.AssignEquipmentsToRoom;
using RoomService.Application.UseCases.Common;
using RoomService.Application.UseCases.Common.Services;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;
using Xunit;

namespace RoomService.UnitTests.Application.AssignEquipmentsToRoom
{
    public class AssignEquipmentsToRoomUseCaseTests
    {
        private readonly Mock<IRoomRepository> _roomRepository = new();
        private readonly Mock<IEquipmentRepository> _equipmentRepository = new();
        private readonly Mock<IEquipmentResponseMapper> _equipmentResponseMapper = new();

        private static Room CreateValidRoom() => new("Sala Azul", 101);

        private static Equipment CreateValidEquipment() =>
            new(EquipmentType.Projetor, "Epson", "SN-123", DateTime.UtcNow.AddDays(-1));

        private AssignEquipmentsToRoomUseCase CreateUseCase()
        {
            _equipmentResponseMapper
                .Setup(m => m.MapEquipmentsAsync(It.IsAny<IEnumerable<RoomEquipment>>()))
                .ReturnsAsync([]);

            return new AssignEquipmentsToRoomUseCase(
                _roomRepository.Object,
                _equipmentRepository.Object,
                _equipmentResponseMapper.Object);
        }

        [Fact]
        public async Task Should_Fail_When_Room_Does_Not_Exist()
        {
            _roomRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Room?)null);
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new AssignEquipmentsToRoomRequest(Guid.NewGuid(), [Guid.NewGuid()]));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Sala não encontrada.");
        }

        [Fact]
        public async Task Should_Fail_When_Equipment_List_Is_Empty()
        {
            var room = CreateValidRoom();
            _roomRepository.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new AssignEquipmentsToRoomRequest(room.Id, []));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Informe ao menos um equipamento.");
        }

        [Fact]
        public async Task Should_Fail_When_Equipment_Id_Is_Empty_Guid()
        {
            var room = CreateValidRoom();
            _roomRepository.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new AssignEquipmentsToRoomRequest(room.Id, [Guid.Empty]));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Há equipamento com identificador inválido.");
        }

        [Fact]
        public async Task Should_Fail_When_Equipment_Does_Not_Exist()
        {
            var room = CreateValidRoom();
            var equipmentId = Guid.NewGuid();
            _roomRepository.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);
            _equipmentRepository.Setup(e => e.GetByIdAsync(equipmentId)).ReturnsAsync((Equipment?)null);
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new AssignEquipmentsToRoomRequest(room.Id, [equipmentId]));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Equipamento não encontrado.");
        }

        [Fact]
        public async Task Should_Fail_And_Allocate_Nothing_When_One_Of_Two_Ids_Does_Not_Exist()
        {
            var room = CreateValidRoom();
            var validId = Guid.NewGuid();
            var missingId = Guid.NewGuid();
            var equipment = CreateValidEquipment();
            _roomRepository.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);
            _equipmentRepository.Setup(e => e.GetByIdAsync(validId)).ReturnsAsync(equipment);
            _equipmentRepository.Setup(e => e.GetByIdAsync(missingId)).ReturnsAsync((Equipment?)null);
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new AssignEquipmentsToRoomRequest(room.Id, [validId, missingId]));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Equipamento não encontrado.");
            room.Equipments.Should().BeEmpty();
            _roomRepository.Verify(r => r.UpdateAsync(It.IsAny<Room>()), Times.Never);
        }

        [Fact]
        public async Task Should_Fail_When_Equipment_Is_Already_Allocated_To_Another_Room()
        {
            var room = CreateValidRoom();
            var equipmentId = Guid.NewGuid();
            _roomRepository.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);
            _equipmentRepository.Setup(e => e.GetByIdAsync(equipmentId)).ReturnsAsync(CreateValidEquipment());
            _roomRepository.Setup(r => r.ExistsByEquipmentIdAsync(equipmentId)).ReturnsAsync(true);
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new AssignEquipmentsToRoomRequest(room.Id, [equipmentId]));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Este equipamento já está alocado a outra sala.");
        }

        [Fact]
        public async Task Should_Fail_When_Equipment_Is_Already_In_This_Room()
        {
            var room = CreateValidRoom();
            var equipmentId = Guid.NewGuid();
            room.AddEquipment(equipmentId);
            _roomRepository.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);
            _equipmentRepository.Setup(e => e.GetByIdAsync(equipmentId)).ReturnsAsync(CreateValidEquipment());
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new AssignEquipmentsToRoomRequest(room.Id, [equipmentId]));

            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("Este equipamento já está na sala.");
            // ExistsByEquipmentIdAsync tambem seria verdadeiro aqui (a propria
            // alocacao existente) — a mensagem certa depende de checar a sala
            // atual primeiro, nao so a existencia da alocacao.
            _roomRepository.Verify(r => r.ExistsByEquipmentIdAsync(equipmentId), Times.Never);
        }

        [Fact]
        public async Task Should_Allocate_Free_Equipment_And_Persist()
        {
            var room = CreateValidRoom();
            var equipmentId = Guid.NewGuid();
            _roomRepository.Setup(r => r.GetByIdAsync(room.Id)).ReturnsAsync(room);
            _equipmentRepository.Setup(e => e.GetByIdAsync(equipmentId)).ReturnsAsync(CreateValidEquipment());
            _roomRepository.Setup(r => r.ExistsByEquipmentIdAsync(equipmentId)).ReturnsAsync(false);
            var useCase = CreateUseCase();

            var result = await useCase.ExecuteAsync(new AssignEquipmentsToRoomRequest(room.Id, [equipmentId]));

            result.IsSuccess.Should().BeTrue();
            result.Value!.Room.Equipments.Should().NotBeNull();
            room.Equipments.Should().ContainSingle(e => e.EquipmentId == equipmentId);
            _roomRepository.Verify(r => r.UpdateAsync(room), Times.Once);
        }
    }
}
