using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;
using RoomService.Application.UseCases.Common;
using RoomService.Application.UseCases.Common.Services;

namespace RoomService.Application.UseCases.GetRoomByName
{
    public class GetRoomByNameUseCase : IGetRoomByNameUseCase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IEquipmentResponseMapper _equipmentResponseMapper;

        public GetRoomByNameUseCase(
            IRoomRepository roomRepository,
            IEquipmentResponseMapper equipmentResponseMapper)
        {
            _roomRepository = roomRepository;
            _equipmentResponseMapper = equipmentResponseMapper; 
        }

        public async Task<Result<GetRoomByNameResponse>> ExecuteAsync(GetRoomByNameRequest request)
        {
            var room = await _roomRepository.GetByNameAsync(request.Name);

            if (room is null)
            {
                return Result<GetRoomByNameResponse>.Failure("Sala não encontrada.");
            }

            var equipmentResponses = await _equipmentResponseMapper.MapEquipmentsAsync(room.Equipments);

            return Result<GetRoomByNameResponse>.Success(
                new GetRoomByNameResponse(
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