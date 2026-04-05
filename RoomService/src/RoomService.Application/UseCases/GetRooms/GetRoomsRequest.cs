namespace RoomService.Application.UseCases.GetRooms;

public record GetRoomsRequest(
    string? Name,
    int? Number
);
