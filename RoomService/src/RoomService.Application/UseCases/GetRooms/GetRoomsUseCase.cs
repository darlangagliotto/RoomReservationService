using RoomService.Domain.Common;
using RoomService.Domain.Entities;
using RoomService.Domain.Repositories;
using RoomService.Application.UseCases.Common;
using RoomService.Application.UseCases.Common.Services;

namespace RoomService.Application.UseCases.GetRooms
{
    public class GetRoomsUseCase : IGetRoomsUseCase
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IEquipmentResponseMapper _equipmentResponseMapper;

        public GetRoomsUseCase(
            IRoomRepository roomRepository,
            IEquipmentResponseMapper equipmentResponseMapper)
        {
            _roomRepository = roomRepository;
            _equipmentResponseMapper = equipmentResponseMapper;
        }

        public async Task<Result<List<RoomResponse>>> ExecuteAsync(GetRoomsRequest request)
        {
            var rooms = await FindRoomsAsync(request);

            if (rooms.Count == 0)
            {
                return Result<List<RoomResponse>>.Failure("No rooms found.");
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

            return Result<List<RoomResponse>>.Success(roomResponses);
        }

        private async Task<List<Room>> FindRoomsAsync(GetRoomsRequest request)
        {
            var requestHasName = !string.IsNullOrWhiteSpace(request.Name);
            var requestHasNumber = request.Number.HasValue;
            
            if (requestHasName && requestHasNumber)
            {
                var room = await _roomRepository.GetByNameAndNumberAsync(request.Name, request.Number.Value);
                return room is not null ? [room] : [];
            }

            if (requestHasNumber)
            {
                var room = await _roomRepository.GetByNumberAsync(request.Number.Value);
                return room is not null ? [room] : [];
            }

            if (requestHasName)
            {
                var room = await _roomRepository.GetByNameAsync(request.Name);
                return room is not null ? [room] : [];
            }

            return await _roomRepository.GetAllAsync();
        }
    }
}
