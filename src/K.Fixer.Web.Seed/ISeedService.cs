namespace K.Fixer.Web.Seed;

public interface ISeedService
{
    Task SeedAsync();
    Task SeedShowcaseAsync();
}