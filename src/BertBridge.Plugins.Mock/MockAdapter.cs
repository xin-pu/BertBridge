using BertBridge.PluginSDK;
using Microsoft.Extensions.Logging;

namespace BertBridge.Plugins.Mock;

/// <summary>
/// Mock 虚拟设备适配器。实现 IDeviceAdapter，提供全静态模拟数据，
/// 无需硬件即可进行全功能开发和调试。
/// </summary>
[AdapterRegistration("MockBERT-800G", "1.0.0", "BertBridge", "MockBERT-800G")]
public sealed class MockAdapter : IDeviceAdapter
{
    private readonly ILogger<MockAdapter> _logger;
    private ConnectionState _state = ConnectionState.Disconnected;
    private ConnectionString? _connectionString;

    // 每通道 PG 配置状态
    private readonly Dictionary<int, PgConfiguration> _pgConfigs = [];
    private readonly Dictionary<int, bool> _pgEnabled = [];
    // 每通道 ED 状态
    private readonly Dictionary<int, bool> _edRunning = [];
    // 模拟计数器
    private readonly Dictionary<int, ulong> _errorCounters = [];
    private readonly Dictionary<int, ulong> _totalCounters = [];
    // GPIO 模拟
    private uint _gpioValue;

    public ConnectionState State => _state;

    public DeviceCapability Capability { get; } = MockCapabilities.Capability;

    public MockAdapter(ILogger<MockAdapter> logger)
    {
        _logger = logger;
    }

    // ── 连接管理 ──

    public bool CanHandle(ConnectionString connectionString)
        => connectionString.Protocol == ConnectionProtocol.Mock;

    public Task ConnectAsync(ConnectionString connectionString, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock 设备连接中: {ConnectionString}", connectionString);
        _state = ConnectionState.Connecting;
        // 模拟连接延迟
        Task.Delay(50, ct).GetAwaiter().GetResult();
        _state = ConnectionState.Connected;
        _connectionString = connectionString;
        _logger.LogInformation("Mock 设备已连接 (模拟)");
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        _logger.LogInformation("Mock 设备断开中...");
        _state = ConnectionState.Disconnected;
        _connectionString = null;
        _pgConfigs.Clear();
        _pgEnabled.Clear();
        _edRunning.Clear();
        _errorCounters.Clear();
        _totalCounters.Clear();
        _logger.LogInformation("Mock 设备已断开");
        return Task.CompletedTask;
    }

    // ── 设备信息 ──

    public Task<DeviceInfo> GetDeviceInfoAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Mock GetDeviceInfo");
        return Task.FromResult(new DeviceInfo(
            Model: "MockBERT-800G",
            SerialNumber: "MOCK-2024-0001",
            FirmwareVersion: "1.0.0",
            BoardType: "MockBoard-X1"
        ));
    }

    // ── 全局配置 ──

    public Task SetBaudRateAsync(uint baudRateKbps, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock SetBaudRateAsync: {Rate} kbps", baudRateKbps);
        return Task.CompletedTask;
    }

    public Task SetSignalModeAsync(string signalMode, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock SetSignalModeAsync: {Mode}", signalMode);
        return Task.CompletedTask;
    }

    public Task SetOperationalModeAsync(string mode, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock SetOperationalModeAsync: {Mode}", mode);
        return Task.CompletedTask;
    }

    // ── PG 操作 ──

    public Task ConfigurePgAsync(int laneIndex, PgConfiguration config, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock ConfigurePgAsync Lane {Lane}: Pattern={Pattern}, Mode={Mode}",
            laneIndex, config.Pattern, config.Mode);
        _pgConfigs[laneIndex] = config;
        return Task.CompletedTask;
    }

    public Task EnablePgAsync(int laneIndex, bool enable, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock EnablePgAsync Lane {Lane}: {Enable}", laneIndex, enable);
        _pgEnabled[laneIndex] = enable;

        if (enable && !_errorCounters.ContainsKey(laneIndex))
        {
            // 初始化计数器
            _errorCounters[laneIndex] = 0;
            _totalCounters[laneIndex] = 0;
        }
        return Task.CompletedTask;
    }

    // ── ED 操作 ──

    public Task ConfigureEdAsync(int laneIndex, EdConfiguration config, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock ConfigureEdAsync Lane {Lane}: Pattern={Pattern}, AutoLock={AutoLock}",
            laneIndex, config.ExpectedPattern, config.AutoLock);
        return Task.CompletedTask;
    }

    public Task StartEdAsync(int laneIndex, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock StartEdAsync Lane {Lane}", laneIndex);
        _edRunning[laneIndex] = true;
        if (!_totalCounters.ContainsKey(laneIndex))
        {
            _errorCounters[laneIndex] = 0;
            _totalCounters[laneIndex] = 1_000_000_000UL; // 10^9 起始总比特
        }
        return Task.CompletedTask;
    }

    public Task StopEdAsync(int laneIndex, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock StopEdAsync Lane {Lane}", laneIndex);
        _edRunning[laneIndex] = false;
        return Task.CompletedTask;
    }

    public Task<EdResult> ReadEdResultAsync(int laneIndex, CancellationToken ct = default)
    {
        if (!_edRunning.GetValueOrDefault(laneIndex))
        {
            return Task.FromResult(new EdResult(
                ErrorCount: 0,
                TotalCount: 0,
                Ber: 0,
                SnrDb: null,
                SignalDetected: false,
                CdrLocked: false,
                PllLocked: false,
                DspReady: false,
                FecLocked: false,
                AlignmentLocked: false,
                Timestamp: DateTime.UtcNow
            ));
        }

        // 模拟递增计数，各通道返回固定 BER ≈ 1e-12
        var totalIncrement = (ulong)Random.Shared.NextInt64(1_000_000_000L, 10_000_000_000L);
        var errorIncrement = totalIncrement / 1_000_000_000_000UL;
        if (errorIncrement == 0) errorIncrement = 1;

        _totalCounters[laneIndex] += totalIncrement;
        _errorCounters[laneIndex] += errorIncrement;

        double ber = _totalCounters[laneIndex] > 0
            ? (double)_errorCounters[laneIndex] / _totalCounters[laneIndex]
            : 0;

        _logger.LogDebug("Mock Lane {Lane}: Errors={Errors}, Total={Total}, BER={BER:E2}",
            laneIndex, _errorCounters[laneIndex], _totalCounters[laneIndex], ber);

        return Task.FromResult(new EdResult(
            ErrorCount: _errorCounters[laneIndex],
            TotalCount: _totalCounters[laneIndex],
            Ber: ber,
            SnrDb: 25.0,  // 固定 SNR 25.0 dB
            SignalDetected: true,
            CdrLocked: true,
            PllLocked: true,
            DspReady: true,
            FecLocked: true,
            AlignmentLocked: true,
            Timestamp: DateTime.UtcNow
        ));
    }

    // ── FEC ──

    public Task<FecStatistics> ReadFecStatisticsAsync(int chipIndex, CancellationToken ct = default)
    {
        _logger.LogDebug("Mock ReadFecStatisticsAsync Chip {Chip}", chipIndex);
        return Task.FromResult(new FecStatistics(
            PreFecBer: 1e-12,
            PostFecBer: 0,
            CorrectableCodewords: 0,
            UncorrectableCodewords: 0,
            SymbolErrors: 0,
            IsLocked: true,
            Timestamp: DateTime.UtcNow
        ));
    }

    // ── GPIO ──

    public Task<uint> ReadGpioAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Mock ReadGpioAsync -> 0x{Value:X8}", _gpioValue);
        return Task.FromResult(_gpioValue);
    }

    public Task WriteGpioAsync(uint mask, uint values, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock WriteGpioAsync: mask=0x{Mask:X8}, values=0x{Values:X8}", mask, values);
        _gpioValue = (_gpioValue & ~mask) | (values & mask);
        return Task.CompletedTask;
    }

    // ── IAsyncDisposable ──

    public async ValueTask DisposeAsync()
    {
        if (_state == ConnectionState.Connected)
        {
            await DisconnectAsync();
        }
        _logger.LogInformation("Mock 适配器已释放");
    }
}
