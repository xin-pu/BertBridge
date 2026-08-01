using BertBridge.Infrastructure.Persistence;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BertBridge.Infrastructure.Tests.Persistence;

public class DevicePersistenceTests
{
    [Fact]
    public async Task EnsureCreatedAsync_BuildsDeviceModel()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<BertBridgeDbContext>()
            .UseSqlite(connection)
            .Options;

        var mediator = Mock.Of<IMediator>();
        await using var db = new BertBridgeDbContext(options, mediator);

        await db.Database.EnsureCreatedAsync();

        Assert.True(await db.Database.CanConnectAsync());
    }
}
