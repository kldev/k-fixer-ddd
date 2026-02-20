using K.Fixer.Domain.PropertyManagement.Events;
using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.PropertyManagement.Aggregates.Company;

public sealed class Company : AggregateRoot<Guid>
{
    public CompanyName  Name         { get; private set; }
    public TaxId        TaxId        { get; }             // niezmienialny po rejestracji
    public ContactEmail ContactEmail { get; private set; }
    public ContactPhone ContactPhone { get; private set; }
    public bool         IsActive     { get; private set; }
    public DateTime     CreatedAt    { get; }

    // EF Core wymaga bezparametrowego konstruktora — prywatny, poza zasięgiem domeny
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Company() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private Company(
        Guid id,
        CompanyName name,
        TaxId taxId,
        ContactEmail contactEmail,
        ContactPhone contactPhone)
        : base(id)
    {
        Name         = name;
        TaxId        = taxId;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        IsActive     = true;
        CreatedAt    = DateTime.UtcNow;
    }

    // ─── Factory ────────────────────────────────────────────────────────────

    public static Result<Company> Create(
        string name,
        string taxIdCountryCode,
        string taxIdNumber,
        string contactEmail,
        string contactPhone, Guid? fixedGuid = null)
    {
        var nameResult  = CompanyName.Create(name);
        var taxIdResult = TaxId.Create(taxIdCountryCode, taxIdNumber);
        var emailResult = ContactEmail.Create(contactEmail);
        var phoneResult = ContactPhone.Create(contactPhone);

        // Zbieramy wszystkie błędy naraz — użytkownik widzi je wszystkie za jednym razem
        var errors = new List<string>();
        if (nameResult.IsFailure)  errors.Add(nameResult.Error!);
        if (taxIdResult.IsFailure) errors.Add(taxIdResult.Error!);
        if (emailResult.IsFailure) errors.Add(emailResult.Error!);
        if (phoneResult.IsFailure) errors.Add(phoneResult.Error!);

        if (errors.Count > 0)
            return Result.Failure<Company>(string.Join("; ", errors));

        var company = new Company(
            fixedGuid ?? Guid.NewGuid(),
            nameResult.Value!,
            taxIdResult.Value!,
            emailResult.Value!,
            phoneResult.Value!);

        // Agregat sam emituje zdarzenie — unikalność nazwy i NIP
        // sprawdzana jest w Application Layer PRZED wywołaniem Create()
        company.RaiseDomainEvent(new CompanyRegisteredEvent(
            company.Id,
            nameResult.Value!.Value,
            taxIdResult.Value!.Value,
            DateTime.UtcNow));

        return Result.Success(company);
    }

    // ─── Metody biznesowe ────────────────────────────────────────────────────

    // Aktualizacja danych kontaktowych — TaxId jest niezmienny, NazwaFirmy przez Rename()
    public Result UpdateContactInfo(string newEmail, string newPhone)
    {
        var emailResult = ContactEmail.Create(newEmail);
        var phoneResult = ContactPhone.Create(newPhone);

        var errors = new List<string>();
        if (emailResult.IsFailure) errors.Add(emailResult.Error!);
        if (phoneResult.IsFailure) errors.Add(phoneResult.Error!);

        if (errors.Count > 0)
            return Result.Failure(string.Join("; ", errors));

        ContactEmail = emailResult.Value!;
        ContactPhone = phoneResult.Value!;

        return Result.Success();
    }

    // Zmiana nazwy — unikalność globalna sprawdzana w Application Layer przed wywołaniem
    public Result Rename(string newName)
    {
        var nameResult = CompanyName.Create(newName);
        if (nameResult.IsFailure) return Result.Failure(nameResult.Error!);

        Name = nameResult.Value!;
        return Result.Success();
    }

    // Dezaktywacja — skutkuje utratą dostępu przez wszystkich użytkowników firmy
    // (obsługa kaskadowa przez handler CompanyDeactivatedEvent)
    public Result Deactivate(Guid deactivatedByAdminId)
    {
        if (!IsActive)
            return Result.Failure("Firma jest już nieaktywna.");

        IsActive = false;

        RaiseDomainEvent(new CompanyDeactivatedEvent(
            Id, Name.Value, deactivatedByAdminId, DateTime.UtcNow));

        return Result.Success();
    }

    // Ponowna aktywacja — przywraca dostęp użytkownikom firmy
    public Result Reactivate()
    {
        if (IsActive)
            return Result.Failure("Firma jest już aktywna.");

        IsActive = true;
        return Result.Success();
    }
}