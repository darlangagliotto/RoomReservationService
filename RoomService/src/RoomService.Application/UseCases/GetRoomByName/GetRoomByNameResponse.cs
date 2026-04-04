using RoomService.Application.UseCases.Common;

namespace RoomService.Application.UseCases.GetRoomByName;
    public record GetRoomByNameResponse(
        RoomResponse Room
    );