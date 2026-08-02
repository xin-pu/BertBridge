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

        // 替换 AddBertBridgeInfrastructure 注册的 DbContext，使用共享的 in-memory 连接。
        // DbContext 和 DbContextOptions 都要移除，否则不同 scope 可能拿到不同的 SQLite :memory: 连接。
        for (var i = services.Count - 1; i >= 0; i--)
        {
            var serviceType = services[i].ServiceType;
            if (serviceType == typeof(BertBridgeDbContext) ||
                serviceType == typeof(DbContextOptions<BertBridgeDbContext>) ||
                serviceType == typeof(DbContextOptions))
            {
                services.RemoveAt(i);
            }
        }

        services.AddDbContext<BertBridgeDbContext>(options =>
            options.UseSqlite(connection));

        var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BertBridgeDbContext>();
        await dbContext.EnsureSchemaAsync();

        return (provider, connection);
    }
}
