using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;
using RoomService.Application.UseCases.Common;
using RoomService.Application.UseCases.Common.Services;

namespace RoomService.Application.UseCases.GetAllRooms
{
    public class GetAllRoomsUseCase : IGetAllRoomsUseCase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IEquipmentResponseMapper _equipmentResponseMapper;

        public GetAllRoomsUseCase(
            IRoomRepository roomRepository,
            IEquipmentResponseMapper equipmentResponseMapper)
        {
            _roomRepository = roomRepository;
            _equipmentResponseMapper = equipmentResponseMapper; 
        }

        public async Task<Result<GetAllRoomsResponse>> ExecuteAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();

            if (rooms.Count == 0)
            {
                return Result<GetAllRoomsResponse>.Failure("Sala não encontrada.");
            }

            var roomResponses = new List<RoomResponse>();

            foreach (var room in rooms)
            {
                var equipmentResponses = await _equipmentResponseMapper.MapEquipmentsAsync(room.Equipments);

                roomResponses.Add(new RoomResponse(
                    room.Id,
                    room.Name,
                    room.Number,
                    equipmentResponses
                ));
            }

            return Result<GetAllRoomsResponse>.Success(new GetAllRoomsResponse(roomResponses));
        }
    }    
}