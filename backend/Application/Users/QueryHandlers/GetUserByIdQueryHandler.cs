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
            .Include(u => u.AuthMethods)
            .Where(u => u.Id == query.UserId && !u.IsDeleted)
            .Select(u => new UserDetailDto(
                u.Id,
                u.Username,
                u.DisplayName,
                u.Role,
                u.IsActive,
                u.LastLoginAt,
                u.CreatedAt,
                u.UpdatedAt,
                u.AuthMethods.Select(a => new AuthMethodDto(
                    a.Identifier,
                    a.Type,
                    a.CreatedAt
                )).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
}
