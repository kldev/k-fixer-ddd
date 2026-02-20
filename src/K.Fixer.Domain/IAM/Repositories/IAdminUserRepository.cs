using K.Fixer.Domain.IAM.Aggregates.AdminUser;

namespace K.Fixer.Domain.IAM.Repositories;

public interface IAdminUserRepository
{
    public Task<AdminUser?> GetById(Guid id, CancellationToken ct = default);
    public Task<AdminUser?> GetByUsername(Username username, CancellationToken ct = default);
}