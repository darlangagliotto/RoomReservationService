using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Domain.Entities;
using AuthService.Domain.Security;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Security
{
    public class JwtTokenGenerator : ITokenGenerator
    {
        private readonly JwtOptions _options;

        public JwtTokenGenerator(JwtOptions options)
        {
            _options = options;
        }

        public TokenResult Generate(User user)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email.Value),
                new(JwtRegisteredClaimNames.UniqueName, user.Name),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            return new TokenResult(tokenValue, expiresAt);
        }

        public TokenResult Generate(Guid userId, string email)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(JwtRegisteredClaimNames.Email, email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            return new TokenResult(tokenValue, expiresAt);
        }
    }
}