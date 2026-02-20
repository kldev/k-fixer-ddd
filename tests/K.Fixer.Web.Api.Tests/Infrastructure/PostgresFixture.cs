using Testcontainers.PostgreSql;

namespace K.Fixer.Web.Api.Tests.Infrastructure;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("k-fix")
        .WithUsername("db_user")
        .WithPassword("db_pass")
        .Build();


    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await InitializeDatabaseAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    protected virtual Task InitializeDatabaseAsync() => Task.CompletedTask;
}

[CollectionDefinition(Name)]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    internal const string Name = "KfixerDatabase";
}