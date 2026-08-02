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

        builder.Ignore(d => d.DeviceId);

        builder.Ignore(d => d.Info);
        builder.Property<string?>("_infoModel").HasColumnName("InfoModel").HasMaxLength(100);
        builder.Property<string?>("_infoSerialNumber").HasColumnName("InfoSerialNumber").HasMaxLength(100);
        builder.Property<string?>("_infoFirmwareVersion").HasColumnName("InfoFirmwareVersion").HasMaxLength(100);
        builder.Property<string?>("_infoBoardType").HasColumnName("InfoBoardType").HasMaxLength(100);

        builder.Ignore(d => d.Connection);
        builder.Property<string?>("_connectionValue").HasColumnName("ConnectionValue").HasMaxLength(500);
        builder.Property<ConnectionProtocol?>("_connectionProtocol")
            .HasColumnName("ConnectionProtocol")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.State)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Ignore(d => d.Capability);
        builder.Property<int?>("_capabilityMaxLanes").HasColumnName("CapabilityMaxLanes");
        builder.Property<bool?>("_capabilitySupportsPAM4").HasColumnName("CapabilitySupportsPAM4");
        builder.Property<bool?>("_capabilitySupportsAdvancedModulation").HasColumnName("CapabilitySupportsAdvancedModulation");
        builder.Property<string?>("_capabilitySupportedPatternsJson").HasColumnName("CapabilitySupportedPatternsJson");
        builder.Property<decimal?>("_capabilityMaxBaudRateGBd").HasColumnName("CapabilityMaxBaudRateGBd");
        builder.Property<bool?>("_capabilitySupportsFec").HasColumnName("CapabilitySupportsFec");
        builder.Property<bool?>("_capabilitySupportsGpio").HasColumnName("CapabilitySupportsGpio");
        builder.Property<int?>("_capabilityFirTapCount").HasColumnName("CapabilityFirTapCount");
        builder.Property<bool?>("_capabilitySupportsJitterInjection").HasColumnName("CapabilitySupportsJitterInjection");

        builder.HasMany(d => d.Lanes)
            .WithOne()
            .HasForeignKey("DeviceId")
            .OnDelete(DeleteBehavior.Cascade);

        var lanesNavigation = builder.Metadata.FindNavigation(nameof(Device.Lanes))
            ?? throw new InvalidOperationException("Device.Lanes navigation is not configured.");

        lanesNavigation.SetField("_lanes");
        lanesNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(d => d.Lanes).AutoInclude();
        builder.Ignore(d => d.DomainEvents);
    }
}
