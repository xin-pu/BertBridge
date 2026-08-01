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
        builder.Ignore(s => s.Configuration);

        // SummaryBer 值对象 → 映射为列
        builder.Ignore(s => s.SummaryBer);

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
