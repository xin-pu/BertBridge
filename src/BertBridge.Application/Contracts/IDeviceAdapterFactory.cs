using BertBridge.PluginSDK;

namespace BertBridge.Application.Contracts;

/// <summary>
/// 设备适配器工厂接口。由 Infrastructure 层实现，
/// 管理 DeviceId → IDeviceAdapter 的映射。
/// </summary>
public interface IDeviceAdapterFactory
{
    /// <summary>获取指定设备的适配器</summary>
    IDeviceAdapter? GetAdapter(Guid deviceId);

    /// <summary>注册适配器（连接时调用）</summary>
    void RegisterAdapter(Guid deviceId, IDeviceAdapter adapter);

    /// <summary>注销适配器（断开时调用）</summary>
    void UnregisterAdapter(Guid deviceId);

    /// <summary>检查是否有适配器能处理指定的连接字符串</summary>
    bool CanHandle(PluginSDK.ConnectionString connectionString);
}
