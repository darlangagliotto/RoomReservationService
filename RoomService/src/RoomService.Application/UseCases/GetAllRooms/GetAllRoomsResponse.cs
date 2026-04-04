using RoomService.Application.UseCases.Common;

namespace RoomService.Application.UseCases.GetAllRooms;
    public record GetAllRoomsResponse(
        List<RoomResponse> Rooms
    );