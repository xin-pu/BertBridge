# 插件连接流程

BertBridge 的设备连接由 CLI/GUI 统一调用 Application 服务，再由 Infrastructure 层完成插件适配和设备通信。

## 当前流程

1. CLI 或 GUI 调用 `IDeviceAppService.ConnectAsync`。
2. Application 同时解析 PluginSDK 连接串和 Domain 连接串。
3. `IDeviceAdapterFactory.CreateAdapter` 根据连接串协议选择可处理的 adapter。
4. Adapter 建立物理或虚拟连接。
5. Application 读取设备信息和能力声明，并写入 Domain 聚合。
6. Repository 将设备和 Lane 元数据写入 SQLite。
7. `IDeviceAdapterFactory.RegisterAdapter` 将在线 adapter 绑定到 `DeviceId`。
8. PG/ED/FEC/GPIO 等后续命令通过 `DeviceId` 找到在线 adapter。

## Mock 连接

开发阶段可通过 CLI 配置启用 Mock 插件：

```json
{
  "Plugins": {
    "EnableMock": true
  }
}
```

验证命令：

```powershell
dotnet run --project src/BertBridge.CLI -- device connect mock://local --name TestMock
dotnet run --project src/BertBridge.CLI -- device list
```

## 当前限制

- 第一阶段 SQLite 只持久化 `Device`、`Lane`、`TestSession` 和 `BerDataPoint` 的核心字段。
- `DeviceInfo`、`DeviceCapability`、`ConnectionString`、`TestConfiguration`、`SummaryBer` 暂未完整落库，避免 EF Core 10 preview 对 nullable complex property 的限制阻塞主流程。
- 下一阶段建议使用 JSON 列或显式 owned entity 恢复这些值对象的完整持久化。
- 动态插件发现目前仍以 adapter 实例为发现结果，后续应演进为 descriptor/type factory，避免多设备场景共享实例。
