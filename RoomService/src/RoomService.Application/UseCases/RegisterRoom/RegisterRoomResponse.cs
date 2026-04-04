using RoomService.Application.UseCases.Common;

namespace RoomService.Application.UseCases.RegisterRoom;

public record RegisterRoomResponse(
    RoomResponse Room
);