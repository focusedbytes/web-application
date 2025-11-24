using System.Text.RegularExpressions;

namespace FocusedBytes.Api.Domain.Users.ValueObjects;

public record Phone
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private Phone(string value)
    {
        Value = value;
    }

    public static Phone Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be empty", nameof(phone));

        var cleaned = Regex.Replace(phone, @"[\s\-\(\)]", "");

        if (!PhoneRegex.IsMatch(cleaned))
            throw new ArgumentException("Invalid phone format", nameof(phone));

        return new Phone(cleaned);
    }

    public override string ToString() => Value;
}
