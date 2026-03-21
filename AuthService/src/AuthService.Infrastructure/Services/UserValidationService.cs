using AuthService.Domain.Services;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Services;

public class UserValidationService : IUserValidationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserValidationService> _logger;

    public UserValidationService(HttpClient httpClient, ILogger<UserValidationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<(bool IsValid, Guid? UserId)> ValidateCredentialsAsync(string email, string password)
    {
        try
        {
            var request = new { email, password };
            
            var response = await _httpClient.PostAsJsonAsync("/api/users/validate-credentials", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("UserService retornou status {StatusCode} ao validar credenciais", response.StatusCode);
                return (false, null);
            }

            var result = await response.Content.ReadFromJsonAsync<ValidateCredentialsResponse>();

            if (result == null)
            {
                _logger.LogWarning("Resposta do UserService foi nula");
                return (false, null);
            }

            return result.IsValid 
                ? (true, result.UserId) 
                : (false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao chamar UserService para validar credenciais");
            return (false, null);
        }
    }

    private class ValidateCredentialsResponse
    {
        public bool IsValid { get; set; }
        public Guid? UserId { get; set; }
    }
}
