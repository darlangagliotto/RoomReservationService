using System.Net;
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

        public async Task<GetRoomResponse?> GetRoomByIdAsync(Guid roomId)
        {
            var response = await _httpClient.GetAsync($"/api/rooms/{roomId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<GetRoomResponse>();
        }

        // Busca por nome e por numero usam o endpoint de consulta que ja existe.
        // Rotas /api/rooms/name/{n} e /api/rooms/number/{n} foram descartadas na
        // spec 002: manter a mesma busca em tres formatos e divida gratuita.
        public Task<GetRoomResponse?> GetRoomByNumberAsync(int roomNumber)
            => QuerySingleAsync($"/api/rooms?number={roomNumber}");

        public Task<GetRoomResponse?> GetRoomByNameAsync(string roomName)
            => QuerySingleAsync($"/api/rooms?name={Uri.EscapeDataString(roomName)}");

        private async Task<GetRoomResponse?> QuerySingleAsync(string path)
        {
            var response = await _httpClient.GetAsync(path);

            // O RoomService responde 400 quando a busca nao encontra nada
            // (ADR-011). Ausencia nao e erro para quem consulta.
            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var rooms = await response.Content.ReadFromJsonAsync<List<GetRoomResponse>>();

            return rooms is { Count: > 0 } ? rooms[0] : null;
        }
    }
}
