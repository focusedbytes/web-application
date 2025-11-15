using FocusedBytes.Api.Application.Common.CQRS;

namespace FocusedBytes.Api.Domain.Users.Commands;

public record DeleteUserCommand(Guid UserId) : ICommand;
