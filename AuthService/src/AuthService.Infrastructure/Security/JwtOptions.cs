namespace AuthService.Infrastructure.Security
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; init; } = string.Empty;
        public string Audience { get; init; } = string.Empty;
        public string Key { get; init; } = string.Empty;
        public int AccessTokenExpirationMinutes { get; init; } = 60;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Issuer))
            {
                throw new InvalidOperationException("JwtOptions: Issuer is required.");
            }
            if (string.IsNullOrWhiteSpace(Key))
            {
                throw new InvalidOperationException("JwtOptions: Key is required.");
            }
            if (Key.Length < 32)
            {
                throw new InvalidOperationException("JwtOptions: Key must be at least 32 characters long.");
            }
            if (AccessTokenExpirationMinutes <= 0)
            {
                throw new InvalidOperationException("JwtOptions: AccessTokenExpirationMinutes must be greater than zero.");
            }
        }
    }
}