using FocusedBytes.Api.Application.Common.CQRS;

namespace FocusedBytes.Api.Domain.Users.Commands;

public record DeactivateUserCommand(Guid UserId, bool IsActive) : ICommand;
