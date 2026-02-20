using K.Fixer.Domain.IAM.Aggregates.User;

namespace K.Fixer.Domain.IAM.Repositories;

public interface IUserRepository
{
    public Task<User?> GetById(Guid id, CancellationToken ct = default);
    public Task<User?> GetByEmail(Email email, CancellationToken ct = default);
}