namespace FocusedBytes.Api.Infrastructure.ReadModels.Entities;

public class AuthMethodReadModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Stored as string to avoid enum issues
    public string? Secret { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserReadModel? User { get; set; }
}
