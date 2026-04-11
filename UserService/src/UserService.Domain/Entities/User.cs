using UserService.Domain.Common;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities
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
            Rename(name);
            ChangeEmail(email);
            ChangePasswordHash(passwordHash);
            IsBlocked = false;            
        }

        protected User() { }

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Name is required.");
            }

            if (name.Trim().Length < 3)
            {
                throw new DomainException("Name must be at least 3 characters long.");
            }

            Name = name.Trim();
        }

        public void ChangeEmail(Email email)
        {
            if (email is null)
            {
                throw new DomainException("Email is required.");
            }

            Email = email;
        }

        public void ChangePasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new DomainException("Password is required.");
            }

            PasswordHash = passwordHash;
        }

        public void Block() => IsBlocked = true;

        public void Unblock() => IsBlocked = false;
    }
}