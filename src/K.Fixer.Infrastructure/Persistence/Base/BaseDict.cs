namespace K.Fixer.Infrastructure.Persistence.Base;

public abstract class BaseDict
{
    public int Id { get; set; }
}

public abstract class BaseTranslatedDict : BaseDict
{
    public string NamePL { get; set; } = string.Empty;
    public string NameEN { get; set; } = string.Empty;
}