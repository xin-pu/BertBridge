namespace BertBridge.PluginSDK;

/// <summary>
/// 设备适配器接口。所有厂商插件必须实现此接口，
/// 将统一的领域操作映射为具体的硬件协议命令。
/// </summary>
public interface IDeviceAdapter : IAsyncDisposable
{
    // ── 连接管理 ──

    /// <summary>连接设备</summary>
    Task ConnectAsync(ConnectionString connectionString, CancellationToken ct = default);

    bool CanHandle(ConnectionString connectionString);

    /// <summary>断开设备</summary>
    Task DisconnectAsync();

    /// <summary>当前连接状态</summary>
    ConnectionState State { get; }

    // ── 设备信息 ──

    /// <summary>获取设备基本信息</summary>
    Task<DeviceInfo> GetDeviceInfoAsync(CancellationToken ct = default);

    // ── 全局配置 ──

    /// <summary>设置波特率 (kbps)</summary>
    Task SetBaudRateAsync(uint baudRateKbps, CancellationToken ct = default);

    /// <summary>设置信号制式 (NRZ/PAM4 等)</summary>
    Task SetSignalModeAsync(string signalMode, CancellationToken ct = default);

    /// <summary>设置工作模式</summary>
    Task SetOperationalModeAsync(string mode, CancellationToken ct = default);

    // ── PG 操作 ──

    /// <summary>配置码型发生器</summary>
    Task ConfigurePgAsync(int laneIndex, PgConfiguration config, CancellationToken ct = default);

    /// <summary>启用/禁用 PG 输出</summary>
    Task EnablePgAsync(int laneIndex, bool enable, CancellationToken ct = default);

    // ── ED 操作 ──

    /// <summary>配置误码检测器</summary>
    Task ConfigureEdAsync(int laneIndex, EdConfiguration config, CancellationToken ct = default);

    /// <summary>启动误码检测</summary>
    Task StartEdAsync(int laneIndex, CancellationToken ct = default);

    /// <summary>停止误码检测</summary>
    Task StopEdAsync(int laneIndex, CancellationToken ct = default);

    /// <summary>读取误码检测结果</summary>
    Task<EdResult> ReadEdResultAsync(int laneIndex, CancellationToken ct = default);

    // ── FEC ──

    /// <summary>读取 FEC 统计</summary>
    Task<FecStatistics> ReadFecStatisticsAsync(int chipIndex, CancellationToken ct = default);

    // ── GPIO ──

    /// <summary>读取 GPIO 端口</summary>
    Task<uint> ReadGpioAsync(CancellationToken ct = default);

    /// <summary>写入 GPIO 端口 (mask + values)</summary>
    Task WriteGpioAsync(uint mask, uint values, CancellationToken ct = default);

    // ── 能力声明 ──

    /// <summary>设备能力声明</summary>
    DeviceCapability Capability { get; }
}
