namespace FocusedBytes.Api.Infrastructure.ReadModels.Entities;

public class UserReadModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public ICollection<AuthMethodReadModel> AuthMethods { get; set; } = new List<AuthMethodReadModel>();
}
