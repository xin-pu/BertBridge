using BertBridge.Domain.TestSession;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BertBridge.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// TestSession 聚合根的 EF Core 实体配置。
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

        builder.Ignore(s => s.Configuration);
        builder.Property<Guid>("_configurationDeviceId").HasColumnName("ConfigurationDeviceId");
        builder.Property<int>("_configurationLaneCount").HasColumnName("ConfigurationLaneCount");
        builder.Property<string>("_configurationPatternsJson").HasColumnName("ConfigurationPatternsJson");
        builder.Property<TimeSpan?>("_configurationDuration").HasColumnName("ConfigurationDuration");
        builder.Property<DateTime>("_configurationSnapshotTime").HasColumnName("ConfigurationSnapshotTime");

        builder.Ignore(s => s.SummaryBer);
        builder.Property<double?>("_summaryBerMantissa").HasColumnName("SummaryBerMantissa");
        builder.Property<int?>("_summaryBerExponent").HasColumnName("SummaryBerExponent");
        builder.Property<ulong?>("_summaryBerErrorCount").HasColumnName("SummaryBerErrorCount");
        builder.Property<ulong?>("_summaryBerTotalCount").HasColumnName("SummaryBerTotalCount");

        builder.HasMany(s => s.DataPoints)
            .WithOne()
            .HasForeignKey("TestSessionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.DataPoints).AutoInclude();

        builder.Ignore(s => s.DomainEvents);
        builder.Ignore(s => s.Duration);
    }
}
