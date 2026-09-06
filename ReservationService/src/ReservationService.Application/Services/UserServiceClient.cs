using System.Net;
using System.Net.Http.Json;

namespace ReservationService.Application.Services
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;

        public UserServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GetUserResponse?> GetUserByIdAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"/api/users/{userId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            // Qualquer outra falha (401 por token expirado, 5xx) e erro de verdade e
            // nao pode virar "usuario nao encontrado". Ver docs/specs/002.
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<GetUserResponse>();
        }
    }
}
