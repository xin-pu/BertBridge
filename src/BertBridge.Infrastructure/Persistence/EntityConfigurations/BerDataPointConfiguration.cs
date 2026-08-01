using BertBridge.Domain.TestSession;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BertBridge.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// BerDataPoint 实体的 EF Core 配置。
/// </summary>
public class BerDataPointConfiguration : IEntityTypeConfiguration<BerDataPoint>
{
    public void Configure(EntityTypeBuilder<BerDataPoint> builder)
    {
        builder.ToTable("BerDataPoints");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.LaneIndex).IsRequired();
        builder.Property(p => p.TimestampMs).IsRequired();
        builder.Property(p => p.ErrorCount).IsRequired();
        builder.Property(p => p.TotalCount).IsRequired();
        builder.Property(p => p.Ber).IsRequired();
        builder.Property(p => p.Snr);

        // Shadow property for FK
        builder.Property<Guid>("TestSessionId").IsRequired();

        builder.HasIndex("TestSessionId", "TimestampMs");
    }
}
