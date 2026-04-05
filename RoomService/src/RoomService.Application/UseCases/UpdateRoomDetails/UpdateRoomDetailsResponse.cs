using RoomService.Application.UseCases.Common;

namespace RoomService.Application.UseCases.UpdateRoomDetails;

    public record UpdateRoomDetailsResponse(
        RoomResponse Room
    );
