using AuthService.Domain.Entities;

namespace AuthService.Domain.Security
{
    public interface ITokenGenerator
    {
        TokenResult Generate(User user);
        TokenResult Generate(Guid userId, string email);
    }
}
