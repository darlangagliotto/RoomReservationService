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
            Validate(name, email, passwordHash);

            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            IsBlocked = false;            
            
        }

        protected User() { }

        public void Block() => IsBlocked = true;

        public void Unblock() => IsBlocked = false;

        private void Validate(string name, Email email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Nome é obrigatório.");
            }

            if (name.Length < 3)
            {
                throw new ArgumentException("Nome deve ter no mínimo 3 caracteres.");
            }

            if (email is null)
            {
                throw new ArgumentNullException(nameof(email), "E-mail é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Senha é obrigatória.");
            }
        }
    }
}