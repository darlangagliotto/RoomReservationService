using RoomService.Application.UseCases.Common;
using RoomService.Application.UseCases.Common.Services;
using RoomService.Domain.Common;
using RoomService.Domain.Repositories;

namespace RoomService.Application.UseCases.GetRoomById
{
    public class GetRoomByIdUseCase : IGetRoomByIdUseCase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IEquipmentResponseMapper _equipmentResponseMapper;

        public GetRoomByIdUseCase(
            IRoomRepository roomRepository,
            IEquipmentResponseMapper equipmentResponseMapper)
        {
            _roomRepository = roomRepository;
            _equipmentResponseMapper = equipmentResponseMapper;
        }

        public async Task<Result<RoomResponse>> ExecuteAsync(GetRoomByIdRequest request)
        {
            var room = await _roomRepository.GetByIdAsync(request.Id);

            if (room is null)
            {
                return Result<RoomResponse>.Failure("Room not found.");
            }

            var equipments = await _equipmentResponseMapper.MapEquipmentsAsync(room.Equipments);

            return Result<RoomResponse>.Success(
                new RoomResponse(
                    room.Id,
                    room.Name,
                    room.Number,
                    room.PlanSlot,
                    equipments
                )
            );
        }
    }
}
