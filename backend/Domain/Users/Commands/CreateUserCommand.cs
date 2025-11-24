using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users.Entities;
using FocusedBytes.Api.Domain.Users.ValueObjects;

namespace FocusedBytes.Api.Domain.Users.Commands;

public record CreateUserCommand(
    string Username,
    UserRole Role,
    string AuthIdentifier,
    AuthMethodType AuthType,
    string? AuthSecret
) : ICommand<Guid>;
