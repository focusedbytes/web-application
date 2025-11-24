using FocusedBytes.Api.Application.Common.CQRS;

namespace FocusedBytes.Api.Application.Users.Queries;

public record GetUserByIdQuery(Guid UserId) : IQuery<UserDetailDto?>;

public record UserDetailDto(
    Guid Id,
    string? Email,
    string Role,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
