using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users.ValueObjects;

namespace FocusedBytes.Api.Domain.Users.Commands;

public record CreateUserCommand(
    string Email,
    string Password,
    UserRole Role
) : ICommand<Guid>;
