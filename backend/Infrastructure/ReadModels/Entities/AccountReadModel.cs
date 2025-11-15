namespace FocusedBytes.Api.Infrastructure.ReadModels.Entities;

public class AccountReadModel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string HashedPassword { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserReadModel? User { get; set; }
}
