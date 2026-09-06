using System.Net.Http.Headers;

namespace ReservationService.Application.Services
{
    /// <summary>
    /// Copia o Bearer do chamador para as chamadas de saida.
    ///
    /// O ReservationService age COMO o usuario que fez a requisicao, herdando
    /// exatamente as permissoes dele. Adequado enquanto a autorizacao e binaria;
    /// revisar se papeis existirem. Ver ADR-012.
    ///
    /// Sem token no contexto, a requisicao segue sem o header e o destino
    /// responde 401 — falha visivel, nao silenciosa.
    /// </summary>
    public class AuthorizationPropagationHandler : DelegatingHandler
    {
        private readonly IAccessTokenProvider _accessTokenProvider;

        public AuthorizationPropagationHandler(IAccessTokenProvider accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = _accessTokenProvider.GetAccessToken();

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
