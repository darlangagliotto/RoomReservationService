using AuthService.Domain.Entities;
using AuthService.Domain.Security;

namespace AuthService.Domain.Services
{
    public class AuthenticationService
    {
        private readonly IPasswordHasher _passwordHasher;

        public AuthenticationService(IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public bool Authenticate(User user, string password)
        {
            if(user.IsBlocked)
            {
                return false;
            }
            return _passwordHasher.Verify(password, user.PasswordHash);
        }        
    }
}