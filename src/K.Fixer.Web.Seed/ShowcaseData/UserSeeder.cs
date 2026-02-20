using K.Fixer.Domain.IAM.Aggregates.AdminUser;
using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Infrastructure.Persistence;
using K.Fixer.Web.Seed.FixedValue;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace K.Fixer.Web.Seed.ShowcaseData;


public class UserSeeder : ISeeder
{
    private readonly AppDbContext _context;
    private const string DefaultPassword = "fixer777";
    private static readonly Random _random = new Random();
    private readonly ILogger<SeedService> _logger;
    // ReSharper disable once ConvertToPrimaryConstructor
    public UserSeeder(AppDbContext context, ILogger<SeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedAdmin();
        await SeedUsers();

    }

    private async Task SeedUsers()
    {
        await SeedCompanyAcmeUsers();

        await SeedCompanyUsers(CompanyGuids.GlobalTech, "globaltech.com");
        await SeedCompanyUsers(CompanyGuids.NovaSoft, "novasoft.com");
        await SeedCompanyUsers(CompanyGuids.IronForge, "ironforge.com");

    }

    private async Task SeedCompanyAcmeUsers()
    {
        var companyId = CompanyGuids.Acme;
        if (await _context.UsersReads.AnyAsync(z => z.CompanyId == companyId))
            return;

        var victoria = User.Create("victoria@acme.com", DefaultPassword, "Victoria Gold",
            UserRole.CompanyAdmin.Value,
            companyId);
        var anna = User.Create("anna@acme.com", DefaultPassword, "Anna Bronze", UserRole.Technician.Value,
            companyId);
        var daniel = User.Create("daniel@acme.com", DefaultPassword, "Daniel Silver", UserRole.Resident.Value,
            companyId);

        _context.Users.Add(victoria.Value!);
        _context.Users.Add(anna.Value!);
        _context.Users.Add(daniel.Value!);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Users for Acme added");
    }

    private async Task SeedCompanyUsers(Guid companyGid, string mailDomain)
    {
        var names = new[] { "Anna", "Bob", "Duncan", "Dave", "Caroline", "Vera", "Justine", "Donald", "Joe" };

        var lastNames = new[]
        {
            "Smith", "Jones", "Williams", "Tribiani", "Johnson", "Star", "Idahoo", "Williams", "Arde", "King", "Queen"
        };

        if (_context.UsersReads.Count(z => z.CompanyId == companyGid) > 5)
            return;
        
        var addWorkers = _random.Next(5, 35);

        List<string> allRoles = [UserRole.Technician.Value, UserRole.Resident.Value];
        var existingEmails = await _context.Users.Select(c => c.Email.Value).ToHashSetAsync();

        for (var i = 0; i < addWorkers; i++)
        {
            var selectedName = names[_random.Next(names.Length)];
            var selectedSurname = lastNames[_random.Next(lastNames.Length)];
            string role = allRoles[_random.Next(allRoles.Count)];
            if (i == 0)
            {
                role = UserRole.CompanyAdmin.Value;
            }

            var email = BuildEmail(selectedName, selectedSurname, mailDomain);

            if (!existingEmails.Add(email)) continue;

            var addUser = User.Create(email, DefaultPassword, selectedName + " " + selectedSurname, role, companyGid);

            if (addUser.IsSuccess)
                _context.Users.Add(addUser.Value!);
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Users for {companyGid} added");
    }

    private string BuildEmail(string selectedName, string selectedSurname, string mailDomain)
    {
        return (selectedName[0] + "." + selectedSurname + "@" + mailDomain).ToLower();
    }

    private async Task SeedAdmin()
    {
        var adminsSeeded = await _context.AdminUsers.AnyAsync(z => z.Username == Username.Create("admin").Value!);
        if (adminsSeeded)
            return;

        var adminUser = AdminUser.Create("admin", DefaultPassword, "Admin Name");
        _context.AdminUsers.Add(adminUser.Value!);
        await _context.SaveChangesAsync();
    }
}