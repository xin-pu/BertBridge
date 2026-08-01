using BertBridge.Infrastructure.Persistence;
using BertBridge.Domain.Device;
using BertBridge.Infrastructure.Persistence.Repositories;
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

    [Fact]
    public async Task DeviceValueObjects_RoundTripThroughSqlite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<BertBridgeDbContext>()
            .UseSqlite(connection)
            .Options;

        var mediator = Mock.Of<IMediator>();

        var connectionString = ConnectionString.Parse("mock://local");
        var device = Device.Create("Mock");
        device.BeginConnect(connectionString);
        device.RegisterDeviceInfo(
            new DeviceInfo("MockBERT-800G", "SN-001", "1.0.0", "MockBoard"),
            new DeviceCapability(
                2,
                supportsPAM4: true,
                supportsAdvancedModulation: false,
                supportedPatterns: ["PRBS7", "PRBS31"],
                maxBaudRateGBd: 56,
                supportsFec: true,
                supportsGpio: false,
                firTapCount: 5,
                supportsJitterInjection: true));
        device.MarkConnected(connectionString);

        await using (var db = new BertBridgeDbContext(options, mediator))
        {
            await db.Database.EnsureCreatedAsync();
            db.Devices.Add(device);
            await db.SaveChangesAsync();
        }

        await using (var db = new BertBridgeDbContext(options, mediator))
        {
            var loaded = await db.Devices.SingleAsync();

            Assert.Equal("MockBERT-800G", loaded.Info?.Model);
            Assert.Equal("SN-001", loaded.Info?.SerialNumber);
            Assert.Equal("mock://local", loaded.Connection?.Value);
            Assert.Equal(ConnectionProtocol.Mock, loaded.Connection?.Protocol);
            Assert.Equal(2, loaded.Capability?.MaxLanes);
            Assert.Contains("PRBS31", loaded.Capability?.SupportedPatterns ?? []);
            Assert.Equal(2, loaded.Lanes.Count);
        }

        await using (var db = new BertBridgeDbContext(options, mediator))
        {
            var repository = new DeviceRepository(db);
            var loaded = await repository.GetByConnectionStringAsync(connectionString);

            Assert.NotNull(loaded);
            Assert.Equal("Mock", loaded.DeviceName);
        }
    }
}
