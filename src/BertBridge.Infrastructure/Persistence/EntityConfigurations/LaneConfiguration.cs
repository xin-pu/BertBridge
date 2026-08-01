using BertBridge.Domain.Device;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BertBridge.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// Lane 实体的 EF Core 配置。
/// </summary>
public class LaneConfiguration : IEntityTypeConfiguration<Lane>
{
    public void Configure(EntityTypeBuilder<Lane> builder)
    {
        builder.ToTable("Lanes");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.LaneIndex).IsRequired();
        builder.Property(l => l.LaneName).IsRequired().HasMaxLength(100);
        builder.Property(l => l.PgEnabled).IsRequired();
        builder.Property(l => l.EdEnabled).IsRequired();
        builder.Property(l => l.CurrentPattern).HasMaxLength(50);

        // Shadow property for FK
        builder.Property<Guid>("DeviceId").IsRequired();
    }
}
