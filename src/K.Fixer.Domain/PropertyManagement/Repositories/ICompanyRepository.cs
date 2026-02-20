using K.Fixer.Domain.PropertyManagement.Aggregates.Company;

namespace K.Fixer.Domain.PropertyManagement.Repositories;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // Sprawdzenie unikalności przed stworzeniem/zmianą nazwy — wywołuje Application Layer
    // excludeId pozwala wykluczyć bieżącą firmę przy zmianie nazwy (Rename)
    Task<bool> IsNameUniqueAsync(
        string name, Guid? excludeId = null, CancellationToken ct = default);

    Task<bool> IsTaxIdUniqueAsync(
        string countryCode, string number, CancellationToken ct = default);

    Task AddAsync(Company company, CancellationToken ct = default);

    // Repozytorium nie ma metody Update — EF Core śledzi zmiany przez Change Tracker
    Task SaveChangesAsync(CancellationToken ct = default);
    
    Task<IReadOnlyList<Company>> GetPagedAsync(
        string? querySearch,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<int> CountAsync(string? querySearch, CancellationToken ct = default);
    
}