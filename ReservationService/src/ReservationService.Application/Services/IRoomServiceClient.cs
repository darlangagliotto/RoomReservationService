namespace ReservationService.Application.Services
{
    public interface IRoomServiceClient
    {
        Task<GetRoomResponse> GetRoomByNumberAsync(int roomNumber);
        Task<GetRoomResponse> GetRoomByNameAsync(string roomName);
        Task<GetRoomResponse?> GetRoomByIdAsync(Guid roomId);
    }
}