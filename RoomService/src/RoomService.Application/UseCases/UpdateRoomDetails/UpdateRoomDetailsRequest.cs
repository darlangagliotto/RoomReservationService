namespace RoomService.Application.UseCases.UpdateRoomDetails;

    public record UpdateRoomDetailsRequest(
        Guid RoomId,
        string? Name,
        int? Number
    );
 