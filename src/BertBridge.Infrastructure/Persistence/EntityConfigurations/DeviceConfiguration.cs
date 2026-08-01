using BertBridge.Domain.Device;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BertBridge.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// Device 聚合根的 EF Core 实体配置。
/// </summary>
public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.DeviceName)
            .IsRequired()
            .HasMaxLength(200);

        // DeviceId 值对象 → 映射为列
        builder.ComplexProperty(d => d.DeviceId, prop =>
        {
            prop.Property(id => id.Value).HasColumnName("DeviceId");
        });

        // DeviceInfo 值对象 → 映射为列
        builder.ComplexProperty(d => d.Info, prop =>
        {
            prop.Property(i => i.Model).HasColumnName("Model").HasMaxLength(100);
            prop.Property(i => i.SerialNumber).HasColumnName("SerialNumber").HasMaxLength(100);
            prop.Property(i => i.FirmwareVersion).HasColumnName("FirmwareVersion").HasMaxLength(50);
            prop.Property(i => i.BoardType).HasColumnName("BoardType").HasMaxLength(50);
        });

        // ConnectionString 值对象 → 映射为列
        builder.ComplexProperty(d => d.Connection, prop =>
        {
            prop.Property(c => c.Value).HasColumnName("ConnectionString").HasMaxLength(500);
            prop.Property(c => c.Protocol).HasColumnName("ConnectionProtocol")
                .HasConversion<string>();
        });

        // ConnectionState → 存储为字符串
        builder.Property(d => d.State)
            .HasConversion<string>()
            .HasMaxLength(20);

        // DeviceCapability 值对象 → 映射为列
        builder.ComplexProperty(d => d.Capability, prop =>
        {
            prop.Property(c => c.MaxLanes).HasColumnName("MaxLanes");
            prop.Property(c => c.SupportsPAM4).HasColumnName("SupportsPAM4");
            prop.Property(c => c.SupportsAdvancedModulation).HasColumnName("SupportsAdvancedModulation");
            prop.Property(c => c.SupportedPatterns).HasColumnName("SupportedPatterns")
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
            prop.Property(c => c.MaxBaudRateGBd).HasColumnName("MaxBaudRateGBd")
                .HasColumnType("decimal(10,3)");
            prop.Property(c => c.SupportsFec).HasColumnName("SupportsFec");
            prop.Property(c => c.SupportsGpio).HasColumnName("SupportsGpio");
            prop.Property(c => c.FirTapCount).HasColumnName("FirTapCount");
            prop.Property(c => c.SupportsJitterInjection).HasColumnName("SupportsJitterInjection");
        });

        // Lanes 集合 → 一对多关系
        builder.HasMany(d => d.Lanes)
            .WithOne()
            .HasForeignKey("DeviceId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Lanes).AutoInclude();

        // 忽略运行时字段
        builder.Ignore(d => d.DomainEvents);
    }
}
