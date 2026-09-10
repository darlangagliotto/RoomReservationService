using RoomService.Application.UseCases.Common;
using RoomService.Application.UseCases.Common.Services;
using RoomService.Domain.Common;
using RoomService.Domain.Repositories;

namespace RoomService.Application.UseCases.RemoveEquipmentFromRoom
{
    public class RemoveEquipmentFromRoomUseCase : IRemoveEquipmentFromRoomUseCase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IEquipmentResponseMapper _equipmentResponseMapper;

        public RemoveEquipmentFromRoomUseCase(
            IRoomRepository roomRepository,
            IEquipmentResponseMapper equipmentResponseMapper)
        {
            _roomRepository = roomRepository;
            _equipmentResponseMapper = equipmentResponseMapper;
        }

        public async Task<Result<RemoveEquipmentFromRoomResponse>> ExecuteAsync(RemoveEquipmentFromRoomRequest request)
        {
            var room = await _roomRepository.GetByIdAsync(request.RoomId);
            if (room is null)
            {
                return Result<RemoveEquipmentFromRoomResponse>.Failure("Sala não encontrada.");
            }

            try
            {
                room.RemoveEquipment(request.EquipmentId);
            }
            catch (DomainException ex)
            {
                return Result<RemoveEquipmentFromRoomResponse>.Failure(ex.Message);
            }

            await _roomRepository.UpdateAsync(room);

            var equipmentResponses = await _equipmentResponseMapper.MapEquipmentsAsync(room.Equipments);

            return Result<RemoveEquipmentFromRoomResponse>.Success(
                new RemoveEquipmentFromRoomResponse(
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
    }
}
