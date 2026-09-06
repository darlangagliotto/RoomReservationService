namespace ReservationService.Application.Services
{
    public interface IRoomServiceClient
    {
        Task<GetRoomResponse?> GetRoomByIdAsync(Guid roomId);
        Task<GetRoomResponse?> GetRoomByNumberAsync(int roomNumber);
        Task<GetRoomResponse?> GetRoomByNameAsync(string roomName);
    }
}
