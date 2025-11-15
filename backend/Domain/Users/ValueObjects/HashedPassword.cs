namespace FocusedBytes.Api.Domain.Users.ValueObjects;

public record HashedPassword
{
    public string Value { get; }

    private HashedPassword(string value)
    {
        Value = value;
    }

    public static HashedPassword Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        if (password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters", nameof(password));

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        return new HashedPassword(hashedPassword);
    }

    public static HashedPassword FromHash(string hash)
    {
        return new HashedPassword(hash);
    }

    public bool Verify(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, Value);
    }

    public override string ToString() => Value;
}
