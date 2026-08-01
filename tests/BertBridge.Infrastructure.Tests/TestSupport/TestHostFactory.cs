using BertBridge.Application;
using BertBridge.Infrastructure;
using BertBridge.Infrastructure.Persistence;
using BertBridge.Plugins.Mock;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BertBridge.Infrastructure.Tests.TestSupport;

internal static class TestHostFactory
{
    public static async Task<(ServiceProvider Services, SqliteConnection Connection)> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = connection.ConnectionString,
                ["Plugins:EnableMock"] = "true"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBertBridgeApplication();
        services.AddBertBridgeInfrastructure(configuration);
        services.AddMockPlugin();
        services.AddDbContext<BertBridgeDbContext>(options => options.UseSqlite(connection));

        var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BertBridgeDbContext>();
        await dbContext.EnsureSchemaAsync();

        return (provider, connection);
    }
}
