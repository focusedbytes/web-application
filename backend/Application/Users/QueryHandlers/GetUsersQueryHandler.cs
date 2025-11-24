using FocusedBytes.Api.Application.Common.CQRS;
using FocusedBytes.Api.Application.Users.Queries;
using FocusedBytes.Api.Infrastructure.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace FocusedBytes.Api.Application.Users.QueryHandlers;

public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, UserListResult>
{
    private readonly ReadModelDbContext _context;

    public GetUsersQueryHandler(ReadModelDbContext context)
    {
        _context = context;
    }

    public async Task<UserListResult> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        var usersQuery = _context.Users
            .Include(u => u.Account)
            .AsQueryable();

        if (!query.IncludeDeleted)
        {
            usersQuery = usersQuery.Where(u => !u.IsDeleted);
        }

        var totalCount = await usersQuery.CountAsync(cancellationToken);

        var users = await usersQuery
            .OrderByDescending(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new UserDto(
                u.Id,
                u.Account!.Email,
                u.Role,
                u.IsActive,
                u.Account.LastLoginAt,
                u.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new UserListResult(users, totalCount, query.Page, query.PageSize, totalPages);
    }
}
