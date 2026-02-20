using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.IAM.Repositories;
using K.Fixer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace K.Fixer.Infrastructure.Auth.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    // ReSharper disable once ConvertToPrimaryConstructor
    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetById(Guid id, CancellationToken ct = default)
    {
        return (await _dbContext.UsersReads.FirstOrDefaultAsync(z => z.Id == id, ct));
    }

    public async Task<User?> GetByEmail(Email email, CancellationToken ct = default)
    {
        return (await _dbContext.Users.FirstOrDefaultAsync(z => z.Email == email, ct));
    }
}