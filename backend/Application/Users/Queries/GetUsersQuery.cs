using FocusedBytes.Api.Application.Common.CQRS;

namespace FocusedBytes.Api.Application.Users.Queries;

public record GetUsersQuery(
    int Page = 1,
    int PageSize = 20,
    bool IncludeDeleted = false
) : IQuery<UserListResult>;

public record UserListResult(
    List<UserDto> Users,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record UserDto(
    Guid Id,
    string Username,
    string? DisplayName,
    string Role,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    List<AuthMethodDto> AuthMethods
);

public record AuthMethodDto(
    string Identifier,
    string Type,
    DateTime CreatedAt
);
