using BertBridge.Domain.Shared;
using System.Text.Json;

namespace BertBridge.Domain.Device;

/// <summary>
/// Device 聚合根。代表一台物理或虚拟 BERT 设备。
/// </summary>
public class Device : BaseAggregateRoot
{
    private string? _infoModel;
    private string? _infoSerialNumber;
    private string? _infoFirmwareVersion;
    private string? _infoBoardType;
    private string? _connectionValue;
    private ConnectionProtocol? _connectionProtocol;
    private int? _capabilityMaxLanes;
    private bool? _capabilitySupportsPAM4;
    private bool? _capabilitySupportsAdvancedModulation;
    private string? _capabilitySupportedPatternsJson;
    private decimal? _capabilityMaxBaudRateGBd;
    private bool? _capabilitySupportsFec;
    private bool? _capabilitySupportsGpio;
    private int? _capabilityFirTapCount;
    private bool? _capabilitySupportsJitterInjection;

    private readonly List<Lane> _lanes = [];

    public DeviceId DeviceId => new(Id);

    public DeviceInfo? Info =>
        _infoModel is null || _infoSerialNumber is null || _infoFirmwareVersion is null || _infoBoardType is null
            ? null
            : new DeviceInfo(_infoModel, _infoSerialNumber, _infoFirmwareVersion, _infoBoardType);

    public ConnectionString? Connection =>
        _connectionValue is null || _connectionProtocol is null
            ? null
            : ConnectionString.Parse(_connectionValue);

    public ConnectionState State { get; private set; }

    public DeviceCapability? Capability =>
        _capabilityMaxLanes is null ||
        _capabilitySupportsPAM4 is null ||
        _capabilitySupportsAdvancedModulation is null ||
        _capabilitySupportedPatternsJson is null ||
        _capabilityMaxBaudRateGBd is null ||
        _capabilitySupportsFec is null ||
        _capabilitySupportsGpio is null ||
        _capabilityFirTapCount is null ||
        _capabilitySupportsJitterInjection is null
            ? null
            : new DeviceCapability(
                _capabilityMaxLanes.Value,
                _capabilitySupportsPAM4.Value,
                _capabilitySupportsAdvancedModulation.Value,
                JsonSerializer.Deserialize<IReadOnlyList<string>>(_capabilitySupportedPatternsJson) ?? [],
                _capabilityMaxBaudRateGBd.Value,
                _capabilitySupportsFec.Value,
                _capabilitySupportsGpio.Value,
                _capabilityFirTapCount.Value,
                _capabilitySupportsJitterInjection.Value);

    public IReadOnlyList<Lane> Lanes => _lanes.AsReadOnly();

    public string DeviceName { get; private set; }

    private Device() : base()
    {
        DeviceName = string.Empty;
        State = ConnectionState.Disconnected;
    }

    private Device(DeviceId deviceId, string deviceName) : base(deviceId.Value)
    {
        DeviceName = !string.IsNullOrWhiteSpace(deviceName)
            ? deviceName
            : throw new ArgumentException("设备名称不能为空。", nameof(deviceName));
        State = ConnectionState.Disconnected;
    }

    public static Device Create(string deviceName)
    {
        return new Device(DeviceId.New(), deviceName);
    }

    public void RegisterDeviceInfo(DeviceInfo info, DeviceCapability capability)
    {
        if (State != ConnectionState.Connecting)
            throw new InvalidOperationException("只能在连接过程中注册设备信息。");

        SetInfo(info ?? throw new ArgumentNullException(nameof(info)));
        SetCapability(capability ?? throw new ArgumentNullException(nameof(capability)));

        _lanes.Clear();
        for (var i = 0; i < capability.MaxLanes; i++)
        {
            _lanes.Add(new Lane(Guid.NewGuid(), i, $"Lane_{i}"));
        }
    }

    public void MarkConnected(ConnectionString connection)
    {
        SetConnection(connection ?? throw new ArgumentNullException(nameof(connection)));
        State = ConnectionState.Connected;

        RaiseDomainEvent(new Events.DeviceConnectedEvent(DeviceId, connection));
    }

    public void BeginConnect(ConnectionString connection)
    {
        if (State == ConnectionState.Connected)
            throw new InvalidOperationException("设备已连接，请先断开。");

        SetConnection(connection ?? throw new ArgumentNullException(nameof(connection)));
        State = ConnectionState.Connecting;
    }

    public void Disconnect()
    {
        if (State == ConnectionState.Disconnected)
            return;

        State = ConnectionState.Disconnected;
        var connection = Connection;
        ClearConnection();

        if (connection != null)
            RaiseDomainEvent(new Events.DeviceDisconnectedEvent(DeviceId, connection));
    }

    public void MarkError(string errorMessage)
    {
        State = ConnectionState.Error;
        RaiseDomainEvent(new Events.DeviceStateChangedEvent(DeviceId, State, errorMessage));
    }

    public Lane GetLane(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= _lanes.Count)
            throw new ArgumentOutOfRangeException(nameof(laneIndex),
                $"通道索引 {laneIndex} 无效。设备共有 {_lanes.Count} 个通道。");

        return _lanes[laneIndex];
    }

    public void EnablePatternGenerator(int laneIndex, string pattern)
    {
        EnsureConnected();
        var lane = GetLane(laneIndex);
        lane.EnablePatternGenerator(pattern);
    }

    public void DisablePatternGenerator(int laneIndex)
    {
        EnsureConnected();
        var lane = GetLane(laneIndex);
        lane.DisablePatternGenerator();
    }

    public void EnableErrorDetector(int laneIndex)
    {
        EnsureConnected();
        var lane = GetLane(laneIndex);
        lane.EnableErrorDetector();
    }

    public void DisableErrorDetector(int laneIndex)
    {
        EnsureConnected();
        var lane = GetLane(laneIndex);
        lane.DisableErrorDetector();
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("设备名称不能为空。", nameof(newName));
        DeviceName = newName;
    }

    private void EnsureConnected()
    {
        if (State != ConnectionState.Connected)
            throw new InvalidOperationException("设备未连接，无法执行操作。");
    }

    private void SetInfo(DeviceInfo info)
    {
        _infoModel = info.Model;
        _infoSerialNumber = info.SerialNumber;
        _infoFirmwareVersion = info.FirmwareVersion;
        _infoBoardType = info.BoardType;
    }

    private void SetConnection(ConnectionString connection)
    {
        _connectionValue = connection.Value;
        _connectionProtocol = connection.Protocol;
    }

    private void ClearConnection()
    {
        _connectionValue = null;
        _connectionProtocol = null;
    }

    private void SetCapability(DeviceCapability capability)
    {
        _capabilityMaxLanes = capability.MaxLanes;
        _capabilitySupportsPAM4 = capability.SupportsPAM4;
        _capabilitySupportsAdvancedModulation = capability.SupportsAdvancedModulation;
        _capabilitySupportedPatternsJson = JsonSerializer.Serialize(capability.SupportedPatterns);
        _capabilityMaxBaudRateGBd = capability.MaxBaudRateGBd;
        _capabilitySupportsFec = capability.SupportsFec;
        _capabilitySupportsGpio = capability.SupportsGpio;
        _capabilityFirTapCount = capability.FirTapCount;
        _capabilitySupportsJitterInjection = capability.SupportsJitterInjection;
    }
}
