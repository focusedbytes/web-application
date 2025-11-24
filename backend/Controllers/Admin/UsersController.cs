using FocusedBytes.Api.Application.DTOs;
using FocusedBytes.Api.Application.Users.CommandHandlers;
using FocusedBytes.Api.Application.Users.Queries;
using FocusedBytes.Api.Application.Users.QueryHandlers;
using FocusedBytes.Api.Domain.Users.Commands;
using FocusedBytes.Api.Domain.Users.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FocusedBytes.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
public class UsersController : ControllerBase
{
    private readonly CreateUserCommandHandler _createUserHandler;
    private readonly UpdateUserCommandHandler _updateUserHandler;
    private readonly UpdateAccountCommandHandler _updateAccountHandler;
    private readonly DeleteUserCommandHandler _deleteUserHandler;
    private readonly DeactivateUserCommandHandler _deactivateUserHandler;
    private readonly GetUsersQueryHandler _getUsersHandler;
    private readonly GetUserByIdQueryHandler _getUserByIdHandler;

    public UsersController(
        CreateUserCommandHandler createUserHandler,
        UpdateUserCommandHandler updateUserHandler,
        UpdateAccountCommandHandler updateAccountHandler,
        DeleteUserCommandHandler deleteUserHandler,
        DeactivateUserCommandHandler deactivateUserHandler,
        GetUsersQueryHandler getUsersHandler,
        GetUserByIdQueryHandler getUserByIdHandler)
    {
        _createUserHandler = createUserHandler;
        _updateUserHandler = updateUserHandler;
        _updateAccountHandler = updateAccountHandler;
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
            request.Email,
            request.Password,
            request.Role);

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

    [HttpPatch("{id}/account")]
    public async Task<IActionResult> UpdateAccount(
        Guid id,
        [FromBody] UpdateAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateAccountCommand(
            id,
            request.Email,
            request.Password);

        await _updateAccountHandler.HandleAsync(command, cancellationToken);

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
