using RoomService.Application.UseCases.Common;

namespace RoomService.Application.UseCases.RemoveEquipmentFromRoom;

public record RemoveEquipmentFromRoomResponse(
    RoomResponse Room
);
