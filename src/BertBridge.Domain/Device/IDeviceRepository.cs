namespace BertBridge.Domain.Device;

/// <summary>
/// Device 仓储接口。定义在 Domain 层，由 Infrastructure 层实现。
/// </summary>
public interface IDeviceRepository
{
    /// <summary>按 ID 获取设备</summary>
    Task<Device?> GetByIdAsync(DeviceId id, CancellationToken ct = default);

    /// <summary>按连接字符串获取设备</summary>
    Task<Device?> GetByConnectionStringAsync(ConnectionString cs, CancellationToken ct = default);

    /// <summary>获取所有已保存的设备配置</summary>
    Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken ct = default);

    /// <summary>新增设备配置</summary>
    Task AddAsync(Device device, CancellationToken ct = default);

    /// <summary>更新设备配置</summary>
    Task UpdateAsync(Device device, CancellationToken ct = default);

    /// <summary>删除设备配置</summary>
    Task DeleteAsync(DeviceId id, CancellationToken ct = default);
}
