using K.Fixer.Domain.Shared;

namespace K.Fixer.Domain.IAM.Aggregates.User;

public sealed class HashedPassword : ValueObject
{
    public string Hash { get; }

    private HashedPassword(string hash) => Hash = hash;

    // Tworzenie nowego hasła — hashuje plain text
    public static Result<HashedPassword> CreateFromPlainText(string? plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return Result.Failure<HashedPassword>("Hasło nie może być puste.");

        if (plainText.Length < 5)
            return Result.Failure<HashedPassword>("Hasło musi mieć co najmniej 8 znaków.");

        var hash = BCrypt.Net.BCrypt.HashPassword(plainText);
        return Result.Success(new HashedPassword(hash));
    }

    // Odtworzenie z bazy danych — hash już jest zahashowany
    public static HashedPassword FromHash(string existingHash)
        => new(existingHash);

    // Weryfikacja — porównuje plain text z hashem
    public bool Verify(string plainText)
        => BCrypt.Net.BCrypt.Verify(plainText, Hash);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Hash;
    }

    // ToString celowo nie zwraca hasha — bezpieczeństwo
    public override string ToString() => "[PROTECTED]";
}