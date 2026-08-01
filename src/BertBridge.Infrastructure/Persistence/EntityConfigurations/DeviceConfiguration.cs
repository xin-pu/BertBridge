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
        builder.Ignore(d => d.DeviceId);

        // DeviceInfo 值对象 → 映射为列
        builder.Ignore(d => d.Info);

        // ConnectionString 值对象 → 映射为列
        builder.Ignore(d => d.Connection);

        // ConnectionState → 存储为字符串
        builder.Property(d => d.State)
            .HasConversion<string>()
            .HasMaxLength(20);

        // DeviceCapability 值对象 → 映射为列
        builder.Ignore(d => d.Capability);

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
