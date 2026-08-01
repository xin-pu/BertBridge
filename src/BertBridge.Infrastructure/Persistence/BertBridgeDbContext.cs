using BertBridge.Domain.Device;
using BertBridge.Domain.Shared;
using BertBridge.Domain.TestSession;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BertBridge.Infrastructure.Persistence;

/// <summary>
/// BertBridge EF Core DbContext。管理 Device 和 TestSession 聚合的持久化。
/// SaveChanges 时自动分发聚合根中注册的领域事件。
/// </summary>
public class BertBridgeDbContext : DbContext
{
    private readonly IMediator _mediator;

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Lane> Lanes => Set<Lane>();
    public DbSet<TestSession> TestSessions => Set<TestSession>();
    public DbSet<BerDataPoint> BerDataPoints => Set<BerDataPoint>();

    public BertBridgeDbContext(DbContextOptions<BertBridgeDbContext> options, IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new EntityConfigurations.DeviceConfiguration());
        modelBuilder.ApplyConfiguration(new EntityConfigurations.LaneConfiguration());
        modelBuilder.ApplyConfiguration(new EntityConfigurations.TestSessionConfiguration());
        modelBuilder.ApplyConfiguration(new EntityConfigurations.BerDataPointConfiguration());
    }

    /// <summary>
    /// 保存时自动分发聚合根中的领域事件。
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // 收集所有聚合根的领域事件
        var domainEvents = ChangeTracker.Entries<IAggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // 先持久化数据
        var result = await base.SaveChangesAsync(ct);

        // 后分发领域事件（通过 MediatR）
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, ct);
        }

        // 清除已分发的事件
        foreach (var entry in ChangeTracker.Entries<IAggregateRoot>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }
}
