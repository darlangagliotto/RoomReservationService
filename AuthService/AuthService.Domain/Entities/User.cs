using AuthService.Domain.ValueObjects;

namespace AuthService.Domain.Entities
{
     public class User
    {
        public Guid Id {get; private set;}
        public required string Name {get; init;}        
        public required Email Email {get; init;}
        public required string PasswordHash {get; init;}
        public bool IsBlocked{get; private set;}

        public User(string name, Email email, string passwordHash)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            IsBlocked = false;            
            
        }

        protected User() { }

        public void Block() => IsBlocked = true;

        public void Unblock() => IsBlocked = false;
    }
}