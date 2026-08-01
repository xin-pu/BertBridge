using BertBridge.Domain.TestSession;
using BertBridge.Infrastructure.Persistence;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BertBridge.Infrastructure.Tests.Persistence;

public class TestSessionPersistenceTests
{
    [Fact]
    public async Task TestSessionConfigurationSummaryAndBerDataPoints_RoundTripThroughSqlite()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<BertBridgeDbContext>()
            .UseSqlite(connection)
            .Options;
        var mediator = Mock.Of<IMediator>();

        var deviceId = Guid.NewGuid();
        var session = TestSession.Create(
            deviceId,
            new TestConfiguration(deviceId, 2, """{"0":"PRBS31","1":"PRBS7"}""", TimeSpan.FromMinutes(10)));
        session.Start();
        session.AddDataPoint(0, errorCount: 3, totalCount: 1_000_000, snr: 24.5);
        session.AddDataPoint(1, errorCount: 1, totalCount: 2_000_000, snr: 25.1);
        session.Complete();

        await using (var db = new BertBridgeDbContext(options, mediator))
        {
            await db.Database.EnsureCreatedAsync();
            db.TestSessions.Add(session);
            await db.SaveChangesAsync();
        }

        await using (var db = new BertBridgeDbContext(options, mediator))
        {
            var loaded = await db.TestSessions.SingleAsync();

            Assert.Equal(deviceId, loaded.Configuration.DeviceId);
            Assert.Equal(2, loaded.Configuration.LaneCount);
            Assert.Contains("PRBS31", loaded.Configuration.PatternsJson);
            Assert.Equal(TimeSpan.FromMinutes(10), loaded.Configuration.Duration);
            Assert.NotNull(loaded.SummaryBer);
            Assert.Equal(3UL, loaded.SummaryBer.ErrorCount);
            Assert.Equal(2, loaded.DataPoints.Count);
            Assert.Contains(loaded.DataPoints, p => p.LaneIndex == 0 && p.Snr == 24.5);
        }
    }
}
