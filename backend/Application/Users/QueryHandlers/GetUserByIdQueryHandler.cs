using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Application.Users.Queries;
using FocusedBytes.Api.Infrastructure.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace FocusedBytes.Api.Application.Users.QueryHandlers;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDetailDto?>
{
    private readonly ReadModelDbContext _context;

    public GetUserByIdQueryHandler(ReadModelDbContext context)
    {
        _context = context;
    }

    public async Task<UserDetailDto?> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Account)
            .Where(u => u.Id == query.UserId && !u.IsDeleted)
            .Select(u => new UserDetailDto(
                u.Id,
                u.Account!.Email,
                u.Role,
                u.IsActive,
                u.Account.LastLoginAt,
                u.CreatedAt,
                u.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
}
