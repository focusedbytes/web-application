using FocusedBytes.Api.Application.DTOs;
using FocusedBytes.Api.Application.Users.CommandHandlers;
using FocusedBytes.Api.Application.Users.Queries;
using FocusedBytes.Api.Application.Users.QueryHandlers;
using FocusedBytes.Api.Domain.Users.Commands;
using Microsoft.AspNetCore.Mvc;

namespace FocusedBytes.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
public class UsersController : ControllerBase
{
    private readonly CreateUserCommandHandler _createUserHandler;
    private readonly UpdateUserCommandHandler _updateUserHandler;
    private readonly UpdateProfileCommandHandler _updateProfileHandler;
    private readonly AddAuthMethodCommandHandler _addAuthMethodHandler;
    private readonly UpdateAuthMethodCommandHandler _updateAuthMethodHandler;
    private readonly RemoveAuthMethodCommandHandler _removeAuthMethodHandler;
    private readonly DeleteUserCommandHandler _deleteUserHandler;
    private readonly DeactivateUserCommandHandler _deactivateUserHandler;
    private readonly GetUsersQueryHandler _getUsersHandler;
    private readonly GetUserByIdQueryHandler _getUserByIdHandler;

    public UsersController(
        CreateUserCommandHandler createUserHandler,
        UpdateUserCommandHandler updateUserHandler,
        UpdateProfileCommandHandler updateProfileHandler,
        AddAuthMethodCommandHandler addAuthMethodHandler,
        UpdateAuthMethodCommandHandler updateAuthMethodHandler,
        RemoveAuthMethodCommandHandler removeAuthMethodHandler,
        DeleteUserCommandHandler deleteUserHandler,
        DeactivateUserCommandHandler deactivateUserHandler,
        GetUsersQueryHandler getUsersHandler,
        GetUserByIdQueryHandler getUserByIdHandler)
    {
        _createUserHandler = createUserHandler;
        _updateUserHandler = updateUserHandler;
        _updateProfileHandler = updateProfileHandler;
        _addAuthMethodHandler = addAuthMethodHandler;
        _updateAuthMethodHandler = updateAuthMethodHandler;
        _removeAuthMethodHandler = removeAuthMethodHandler;
        _deleteUserHandler = deleteUserHandler;
        _deactivateUserHandler = deactivateUserHandler;
        _getUsersHandler = getUsersHandler;
        _getUserByIdHandler = getUserByIdHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUsersQuery(page, pageSize, includeDeleted);
        var result = await _getUsersHandler.HandleAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _getUserByIdHandler.HandleAsync(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateUserCommand(
            request.Username,
            request.Role,
            request.AuthIdentifier,
            request.AuthType,
            request.AuthSecret);

        var userId = await _createUserHandler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetUserById), new { id = userId }, new { id = userId });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateUserCommand(id, request.Role);
        await _updateUserHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id}/profile")]
    public async Task<IActionResult> UpdateProfile(
        Guid id,
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateProfileCommand(id, request.DisplayName);
        await _updateProfileHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    [HttpPost("{id}/auth-methods")]
    public async Task<IActionResult> AddAuthMethod(
        Guid id,
        [FromBody] AddAuthMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AddAuthMethodCommand(
            id,
            request.Identifier,
            request.Type,
            request.Secret);

        await _addAuthMethodHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id}/auth-methods")]
    public async Task<IActionResult> UpdateAuthMethod(
        Guid id,
        [FromBody] UpdateAuthMethodRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateAuthMethodCommand(
            id,
            request.Identifier,
            request.NewSecret);

        await _updateAuthMethodHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}/auth-methods/{identifier}")]
    public async Task<IActionResult> RemoveAuthMethod(
        Guid id,
        string identifier,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveAuthMethodCommand(id, identifier);
        await _removeAuthMethodHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(
        Guid id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivateUserCommand(id, request.IsActive);
        await _deactivateUserHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteUserCommand(id);
        await _deleteUserHandler.HandleAsync(command, cancellationToken);

        return NoContent();
    }
}
