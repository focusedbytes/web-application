using FocusedBytes.Api.Application.Common.CQRS;

namespace FocusedBytes.Api.Domain.Users.Commands;

public record UpdateAccountCommand(
    Guid UserId,
    string? Email = null,
    string? Password = null
) : ICommand;
