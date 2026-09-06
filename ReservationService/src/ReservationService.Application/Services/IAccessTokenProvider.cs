namespace ReservationService.Application.Services
{
    /// <summary>
    /// Fonte do token do chamador, para propagacao nas chamadas a outros servicos.
    ///
    /// Abstraida aqui para a camada Application nao depender de ASP.NET: a
    /// implementacao que le o HttpContext vive na Api. Ver docs/specs/002.
    /// </summary>
    public interface IAccessTokenProvider
    {
        string? GetAccessToken();
    }
}
