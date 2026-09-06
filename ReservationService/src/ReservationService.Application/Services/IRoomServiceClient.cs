namespace ReservationService.Application.Services
{
    public interface IRoomServiceClient
    {
        Task<GetRoomResponse?> GetRoomByIdAsync(Guid roomId);
        Task<GetRoomResponse?> GetRoomByNumberAsync(int roomNumber);
        Task<GetRoomResponse?> GetRoomByNameAsync(string roomName);

        /// <summary>
        /// Todas as salas, numa chamada so. Base da disponibilidade: sem isso
        /// seria uma chamada por sala. Ver docs/specs/003.
        /// </summary>
        Task<List<GetRoomResponse>> GetAllRoomsAsync();
    }
}
