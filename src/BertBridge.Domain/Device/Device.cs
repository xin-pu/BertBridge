using BertBridge.Domain.Shared;

namespace BertBridge.Domain.Device;

/// <summary>
/// Device 聚合根。代表一个物理或虚拟 BERT 设备，
/// 管理连接生命周期、通道配置和设备状态。
/// </summary>
public class Device : BaseAggregateRoot
{
    /// <summary>设备唯一标识</summary>
    public DeviceId DeviceId { get; }

    /// <summary>设备基本信息</summary>
    public DeviceInfo? Info { get; private set; }

    /// <summary>连接字符串</summary>
    public ConnectionString? Connection { get; private set; }

    /// <summary>当前连接状态</summary>
    public ConnectionState State { get; private set; }

    /// <summary>设备能力声明</summary>
    public DeviceCapability? Capability { get; private set; }

    /// <summary>通道列表（只读）</summary>
    private readonly List<Lane> _lanes = [];
    public IReadOnlyList<Lane> Lanes => _lanes.AsReadOnly();

    /// <summary>当前设备名称/别名</summary>
    public string DeviceName { get; private set; }

    private Device(DeviceId deviceId, string deviceName) : base(deviceId.Value)
    {
        DeviceId = deviceId;
        DeviceName = !string.IsNullOrWhiteSpace(deviceName)
            ? deviceName
            : throw new ArgumentException("设备名称不能为空。", nameof(deviceName));
        State = ConnectionState.Disconnected;
    }

    /// <summary>
    /// 创建新设备聚合（不指定 ID，自动生成）。
    /// </summary>
    public static Device Create(string deviceName)
    {
        var device = new Device(DeviceId.New(), deviceName);
        return device;
    }

    /// <summary>
    /// 注册设备信息和能力声明（通常在连接成功后由适配器回调）。
    /// </summary>
    public void RegisterDeviceInfo(DeviceInfo info, DeviceCapability capability)
    {
        if (State != ConnectionState.Connecting)
            throw new InvalidOperationException("只能在连接过程中注册设备信息。");

        Info = info ?? throw new ArgumentNullException(nameof(info));
        Capability = capability ?? throw new ArgumentNullException(nameof(capability));

        // 根据能力创建 Lane
        _lanes.Clear();
        for (int i = 0; i < capability.MaxLanes; i++)
        {
            _lanes.Add(new Lane(Guid.NewGuid(), i, $"Lane_{i}"));
        }
    }

    /// <summary>
    /// 标记设备为已连接。
    /// </summary>
    public void MarkConnected(ConnectionString connection)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        State = ConnectionState.Connected;

        RaiseDomainEvent(new Events.DeviceConnectedEvent(DeviceId, connection));
    }

    /// <summary>
    /// 开始连接流程。
    /// </summary>
    public void BeginConnect(ConnectionString connection)
    {
        if (State == ConnectionState.Connected)
            throw new InvalidOperationException("设备已连接，请先断开。");

        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        State = ConnectionState.Connecting;
    }

    /// <summary>
    /// 断开设备连接。
    /// </summary>
    public void Disconnect()
    {
        if (State == ConnectionState.Disconnected)
            return;

        State = ConnectionState.Disconnected;
        var connection = Connection;
        Connection = null;

        if (connection != null)
            RaiseDomainEvent(new Events.DeviceDisconnectedEvent(DeviceId, connection));
    }

    /// <summary>
    /// 标记连接错误。
    /// </summary>
    public void MarkError(string errorMessage)
    {
        State = ConnectionState.Error;
        RaiseDomainEvent(new Events.DeviceStateChangedEvent(DeviceId, State, errorMessage));
    }

    /// <summary>
    /// 获取指定索引的通道，若不存在则抛出异常。
    /// </summary>
    public Lane GetLane(int laneIndex)
    {
        if (laneIndex < 0 || laneIndex >= _lanes.Count)
            throw new ArgumentOutOfRangeException(nameof(laneIndex),
                $"通道索引 {laneIndex} 无效。设备共有 {_lanes.Count} 个通道。");

        return _lanes[laneIndex];
    }

    /// <summary>
    /// 启用指定通道的码型发生器。
    /// </summary>
    public void EnablePatternGenerator(int laneIndex, string pattern)
    {
        EnsureConnected();
        var lane = GetLane(laneIndex);
        lane.EnablePatternGenerator(pattern);
    }

    /// <summary>
    /// 禁用指定通道的码型发生器。
    /// </summary>
    public void DisablePatternGenerator(int laneIndex)
    {
        EnsureConnected();
        var lane = GetLane(laneIndex);
        lane.DisablePatternGenerator();
    }

    /// <summary>
    /// 启用指定通道的误码检测器。
    /// </summary>
    public void EnableErrorDetector(int laneIndex)
    {
        EnsureConnected();
        var lane = GetLane(laneIndex);
        lane.EnableErrorDetector();
    }

    /// <summary>
    /// 禁用指定通道的误码检测器。
    /// </summary>
    public void DisableErrorDetector(int laneIndex)
    {
        EnsureConnected();
        var lane = GetLane(laneIndex);
        lane.DisableErrorDetector();
    }

    /// <summary>
    /// 更改设备名称。
    /// </summary>
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
}
