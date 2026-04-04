using RoomService.Application.UseCases.Common;

namespace RoomService.Application.UseCases.GetRoomByNumber;
    public record GetRoomByNumberResponse(
        RoomResponse Room
    );