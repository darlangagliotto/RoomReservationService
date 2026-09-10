using RoomService.Application.UseCases.Common;
using RoomService.Application.UseCases.Common.Services;
using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;

namespace RoomService.Application.UseCases.AssignEquipmentsToRoom
{
    public class AssignEquipmentsToRoomUseCase : IAssignEquipmentsToRoomUseCase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly IEquipmentResponseMapper _equipmentResponseMapper;

        public AssignEquipmentsToRoomUseCase(
            IRoomRepository roomRepository,
            IEquipmentRepository equipmentRepository,
            IEquipmentResponseMapper equipmentResponseMapper)
        {
            _roomRepository = roomRepository;
            _equipmentRepository = equipmentRepository;
            _equipmentResponseMapper = equipmentResponseMapper;
        }

        public async Task<Result<AssignEquipmentsToRoomResponse>> ExecuteAsync(AssignEquipmentsToRoomRequest request)
        {
            var room = await _roomRepository.GetByIdAsync(request.RoomId);
            if (room is null)
            {
                return Result<AssignEquipmentsToRoomResponse>.Failure("Sala não encontrada.");
            }

            var validationResult = await ValidateAsync(request, room);
            if (!validationResult.IsSuccess)
            {
                return Result<AssignEquipmentsToRoomResponse>.Failure(validationResult.Error!);
            }

            try
            {
                AllocateEquipments(room, request.EquipmentIds.Distinct());
            }
            catch (DomainException ex)
            {
                return Result<AssignEquipmentsToRoomResponse>.Failure(ex.Message);
            }

            await _roomRepository.UpdateAsync(room);

            var equipmentResponses = await _equipmentResponseMapper.MapEquipmentsAsync(room.Equipments);

            return Result<AssignEquipmentsToRoomResponse>.Success(
                new AssignEquipmentsToRoomResponse(
                    new RoomResponse(
                        room.Id,
                        room.Name,
                        room.Number,
                        room.PlanSlot,
                        equipmentResponses
                    )
                )
            );
        }

        // Validado tudo antes de gravar qualquer coisa: com uma lista de ids a
        // operacao e atomica (ver docs/specs/007#5). Ordem: existencia do
        // equipamento antes de alocacao, mesma disciplina do CreateReservationUseCase.
        private async Task<Result<bool>> ValidateAsync(AssignEquipmentsToRoomRequest request, Room room)
        {
            if (request.EquipmentIds is null || request.EquipmentIds.Count == 0)
            {
                return Result<bool>.Failure("Informe ao menos um equipamento.");
            }

            if (request.EquipmentIds.Any(id => id == Guid.Empty))
            {
                return Result<bool>.Failure("Há equipamento com identificador inválido.");
            }

            var distinctIds = request.EquipmentIds.Distinct().ToList();

            foreach (var equipmentId in distinctIds)
            {
                var equipment = await _equipmentRepository.GetByIdAsync(equipmentId);
                if (equipment is null)
                {
                    return Result<bool>.Failure("Equipamento não encontrado.");
                }
            }

            foreach (var equipmentId in distinctIds)
            {
                if (room.Equipments.Any(e => e.EquipmentId == equipmentId))
                {
                    return Result<bool>.Failure("Este equipamento já está na sala.");
                }

                var allocatedElsewhere = await _roomRepository.ExistsByEquipmentIdAsync(equipmentId);
                if (allocatedElsewhere)
                {
                    return Result<bool>.Failure("Este equipamento já está alocado a outra sala.");
                }
            }

            return Result<bool>.Success(true);
        }

        private static void AllocateEquipments(Room room, IEnumerable<Guid> equipmentIds)
        {
            foreach (var equipmentId in equipmentIds)
            {
                room.AddEquipment(equipmentId);
            }
        }
    }
}
