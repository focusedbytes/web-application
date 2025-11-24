using FocusedBytes.Api.Application.Common.CQRS;

namespace FocusedBytes.Api.Domain.Users.Commands;

public record UpdateAuthMethodCommand(
    Guid UserId,
    string Identifier,
    string? NewSecret
) : ICommand;
