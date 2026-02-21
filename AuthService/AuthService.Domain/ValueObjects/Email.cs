using System.Text.RegularExpressions;

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
            if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("E-mail inválido");

            Value = value.ToLower().Trim();
        }
        public override string ToString() => Value;
    }
}