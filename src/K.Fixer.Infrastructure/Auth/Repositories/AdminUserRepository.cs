using K.Fixer.Domain.IAM.Aggregates.AdminUser;
using K.Fixer.Domain.IAM.Repositories;
using K.Fixer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace K.Fixer.Infrastructure.Auth.Repositories;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly AppDbContext _dbContext;

    // ReSharper disable once ConvertToPrimaryConstructor
    public AdminUserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<AdminUser?> GetById(Guid id, CancellationToken ct)
    {
        return (await _dbContext.AdminUsers.FirstOrDefaultAsync(z => z.Id == id, ct));
    }

    public async Task<AdminUser?> GetByUsername(Username username, CancellationToken ct)
    {
        return (await _dbContext.AdminUsers.FirstOrDefaultAsync(z => z.Username == username, ct));
    }
}