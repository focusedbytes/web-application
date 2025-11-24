using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Domain.Users.Entities;

namespace FocusedBytes.Api.Domain.Users.Commands;

public record AddAuthMethodCommand(
    Guid UserId,
    string Identifier,
    AuthMethodType Type,
    string? Secret
) : ICommand;
