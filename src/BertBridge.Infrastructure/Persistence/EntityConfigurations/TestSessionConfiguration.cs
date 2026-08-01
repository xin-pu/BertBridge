using BertBridge.Domain.TestSession;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BertBridge.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// TestSession 聚合根的 EF Core 配置。
/// </summary>
public class TestSessionConfiguration : IEntityTypeConfiguration<TestSession>
{
    public void Configure(EntityTypeBuilder<TestSession> builder)
    {
        builder.ToTable("TestSessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.DeviceId).IsRequired();
        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.StartedAt);
        builder.Property(s => s.CompletedAt);
        builder.Property(s => s.Notes).HasMaxLength(2000);

        // TestConfiguration 值对象 → 映射为列
        builder.ComplexProperty(s => s.Configuration, prop =>
        {
            prop.Property(c => c.DeviceId).HasColumnName("ConfigDeviceId");
            prop.Property(c => c.LaneCount).HasColumnName("ConfigLaneCount");
            prop.Property(c => c.PatternsJson).HasColumnName("ConfigPatternsJson");
            prop.Property(c => c.Duration).HasColumnName("ConfigDuration");
            prop.Property(c => c.SnapshotTime).HasColumnName("ConfigSnapshotTime");
        });

        // SummaryBer 值对象 → 映射为列
        builder.ComplexProperty(s => s.SummaryBer, prop =>
        {
            prop.Property(b => b.Mantissa).HasColumnName("SummaryBerMantissa");
            prop.Property(b => b.Exponent).HasColumnName("SummaryBerExponent");
            prop.Property(b => b.ErrorCount).HasColumnName("SummaryErrorCount");
            prop.Property(b => b.TotalCount).HasColumnName("SummaryTotalCount");
        });

        // DataPoints 集合
        builder.HasMany(s => s.DataPoints)
            .WithOne()
            .HasForeignKey("TestSessionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.DataPoints).AutoInclude();

        builder.Ignore(s => s.DomainEvents);
        builder.Ignore(s => s.Duration);
    }
}
