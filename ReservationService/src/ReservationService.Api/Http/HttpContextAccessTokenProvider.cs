using System.Net.Http.Headers;
using ReservationService.Application.Services;

namespace ReservationService.Api.Http
{
    /// <summary>
    /// Le o Bearer da requisicao em curso. Unico ponto do servico que conhece
    /// HttpContext para efeito de propagacao de token.
    /// </summary>
    public class HttpContextAccessTokenProvider : IAccessTokenProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextAccessTokenProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetAccessToken()
        {
            var header = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(header))
            {
                return null;
            }

            return AuthenticationHeaderValue.TryParse(header, out var parsed)
                && string.Equals(parsed.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
                    ? parsed.Parameter
                    : null;
        }
    }
}
