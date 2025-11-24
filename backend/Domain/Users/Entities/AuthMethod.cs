namespace FocusedBytes.Api.Domain.Users.Entities;

public class AuthMethod
{
    public string Identifier { get; private set; }
    public AuthMethodType Type { get; private set; }
    public string? Secret { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AuthMethod()
    {
        Identifier = string.Empty;
    }

    public AuthMethod(string identifier, AuthMethodType type, string? secret)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identifier cannot be empty", nameof(identifier));

        Identifier = identifier;
        Type = type;
        Secret = secret;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateSecret(string? newSecret)
    {
        Secret = newSecret;
    }
}
