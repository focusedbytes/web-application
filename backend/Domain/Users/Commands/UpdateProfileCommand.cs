using FocusedBytes.Api.Application.Common.CQRS;

namespace FocusedBytes.Api.Domain.Users.Commands;

public record UpdateProfileCommand(
    Guid UserId,
    string? DisplayName
) : ICommand;
