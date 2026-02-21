using AuthService.Domain.ValueObjects;

namespace AuthService.Domain.Entities
{
     public class User
    {
        public Guid Id {get; private set;}
        public string Name {get; private set;}        
        public Email Email {get; private set;}
        public string PasswordHash {get; private set;}
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