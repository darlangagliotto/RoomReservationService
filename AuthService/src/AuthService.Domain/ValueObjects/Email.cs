using System.Net.Mail;

namespace AuthService.Domain.ValueObjects
{
    public sealed record Email
    {
        public string Value { get; }
        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Necessário informar o e-mail");
            }
            try
            {
                var mailAddress = new MailAddress(value);
                Value = mailAddress.Address.ToLower().Trim();
            }
            catch
            {
                throw new ArgumentException("E-mail inválido");
            }
        }
        public override string ToString() => Value;
    }
}