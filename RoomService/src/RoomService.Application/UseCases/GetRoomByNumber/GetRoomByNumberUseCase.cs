using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;
using RoomService.Application.UseCases.Common;
using RoomService.Application.UseCases.Common.Services;

namespace RoomService.Application.UseCases.GetRoomByNumber
{
    public class GetRoomByNumberUseCase : IGetRoomByNumberUseCase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IEquipmentResponseMapper _equipmentResponseMapper;

        public GetRoomByNumberUseCase(
            IRoomRepository roomRepository,
            IEquipmentResponseMapper equipmentResponseMapper)
        {
            _roomRepository = roomRepository;
            _equipmentResponseMapper = equipmentResponseMapper; 
        }

        public async Task<Result<GetRoomByNumberResponse>> ExecuteAsync(GetRoomByNumberRequest request)
        {
            var room = await _roomRepository.GetByNumberAsync(request.Number);

            if (room is null)
            {
                return Result<GetRoomByNumberResponse>.Failure("Sala não encontrada.");
            }

            var equipmentResponses = await _equipmentResponseMapper.MapEquipmentsAsync(room.Equipments);

            return Result<GetRoomByNumberResponse>.Success(
                new GetRoomByNumberResponse(
                    new RoomResponse(
                        room.Id,
                        room.Name,
                        room.Number,
                        equipmentResponses
                    )
                )
            );
        }
    }    
}