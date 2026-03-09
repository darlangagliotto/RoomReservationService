namespace AuthService.Domain.Security
{
    public record TokenResult(
        string Token,
        DateTime ExpiresAt
    );
}