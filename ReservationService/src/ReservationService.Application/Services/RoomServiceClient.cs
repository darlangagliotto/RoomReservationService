using System.Net.Http.Json;

namespace ReservationService.Application.Services
{
    public class RoomServiceClient : IRoomServiceClient
    {
        private readonly HttpClient _httpClient;

        public RoomServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GetRoomResponse?> GetRoomByNameAsync(string roomName)
        {
            var response = await _httpClient.GetAsync($"/api/rooms/name/{roomName}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<GetRoomResponse>();
        }

        public async Task<GetRoomResponse?> GetRoomByNumberAsync(int roomNumber)
        {
            var response = await _httpClient.GetAsync($"/api/rooms/number/{roomNumber}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<GetRoomResponse>();
        }

        public async Task<GetRoomResponse?> GetRoomByIdAsync(Guid roomId)
        {
            var response = await _httpClient.GetAsync($"/api/rooms/id/{roomId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<GetRoomResponse>();
        }
    }
}