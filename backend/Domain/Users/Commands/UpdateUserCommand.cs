using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users.ValueObjects;

namespace FocusedBytes.Api.Domain.Users.Commands;

public record UpdateUserCommand(
    Guid UserId,
    UserRole Role
) : ICommand;
