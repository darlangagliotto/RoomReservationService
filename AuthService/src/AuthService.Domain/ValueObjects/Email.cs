using System.Text.RegularExpressions;
using AuthService.Domain.Common;

namespace AuthService.Domain.ValueObjects
{
    public sealed record Email
    {
        public string Value { get; }

        public static readonly Regex EmailRegex =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException("Email is required");
            }

            var normalized = value.Trim().ToLowerInvariant();

            if (!EmailRegex.IsMatch(normalized))
            {
                throw new DomainException("Invalid email");
            }

            Value = normalized;
        }

        public override string ToString() => Value;
    }
}
