using FocusedBytes.Api.Application.Common.CQRS;

namespace FocusedBytes.Api.Domain.Users.Commands;

public record RemoveAuthMethodCommand(
    Guid UserId,
    string Identifier
) : ICommand;
