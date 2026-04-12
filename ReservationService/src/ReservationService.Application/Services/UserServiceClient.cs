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
            var response = await _httpClient.GetAsync($"/api/users/id/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            return await response.Content.ReadFromJsonAsync<GetUserResponse>();
        }
    }
}