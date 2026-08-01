# BertBridge Encoding And Core Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 统一仓库 UTF-8 编码，并重构 BertBridge 的设备连接、插件适配、持久化和 CLI 主流程，使 Mock 设备能稳定跑通，为后续 WPF/DevExpress/MVVM GUI 打基础。

**Architecture:** 保持现有 Domain/Application/Infrastructure/CLI/GUI/PluginSDK 分层，但收紧边界：Domain 只表达业务状态和规则，Application 只编排用例，Infrastructure 负责 EF、插件发现和设备通信。插件系统从“按 DeviceId 查已存在 Adapter”改为“按连接串解析并创建 Adapter，连接成功后绑定到 DeviceId”。

**Tech Stack:** .NET 10, WPF, DevExpress, MVVM, EF Core SQLite, System.CommandLine, Serilog, McMaster.NETCore.Plugins, xUnit 测试项目。

## Global Constraints

- 所有新增或修改的源码、XAML、JSON、Markdown 文件必须保存为 UTF-8。
- 不批量重写业务逻辑；每个任务只处理一个可验证的主问题。
- 保留现有项目分层，不把 GUI、CLI 或 Infrastructure 逻辑放入 Domain。
- CLI 和 GUI 共享 Application 服务，不各自直接调用插件或 EF Core。
- 插件 SDK 保持向第三方厂商开放，变更接口时要有 Mock 插件同步验证。
- SQLite 用于本地设备、会话、测试数据和用户设置；运行时连接对象不直接持久化。
- 每个任务结束必须运行指定验证命令，并记录失败原因或通过结果。

---

## Tasks

### Task 1: UTF-8 Policy And Encoding Baseline

- [ ] 增加 `.editorconfig`，固定 UTF-8、CRLF、缩进和 Markdown 策略。
- [ ] 创建 `docs/architecture/encoding-policy.md`，说明编码规范、转换原则和验证方式。
- [ ] 检查当前文件编码状态，区分文件真实乱码和终端显示编码问题。
- [ ] 运行 `dotnet build BertBridge.sln` 验证编码配置不影响构建。

### Task 2: EF Core Device Mapping Startup Fix

- [ ] 创建 `tests/BertBridge.Infrastructure.Tests`。
- [ ] 写 `DevicePersistenceTests.EnsureCreatedAsync_BuildsDeviceModel`，先复现 EF 模型启动失败。
- [ ] 修复 `Device` 聚合的 EF 物化方式和 `DeviceId` 映射策略。
- [ ] 运行 `dotnet test tests/BertBridge.Infrastructure.Tests` 和 `dotnet build BertBridge.sln`。

### Task 3: Adapter Factory Connection Flow

- [ ] 修改 `IDeviceAdapterFactory`，增加按连接串创建 adapter 的接口。
- [ ] 修改 `DeviceAdapterFactory`，实现连接串匹配、在线实例注册和异步释放。
- [ ] 修改 `DeviceAppService.ConnectAsync`，先创建 adapter，再连接设备，连接成功后绑定 DeviceId。
- [ ] 修改 `DisconnectAsync`，通过工厂释放 adapter。
- [ ] 用 `dotnet run --project src/BertBridge.CLI -- device connect mock://local --name TestMock` 验证。

### Task 4: Plugin Discovery And Mock Registration Cleanup

- [ ] 去掉 CLI 中 `|| true` 的强制 Mock 逻辑。
- [ ] 在 Infrastructure DI 中合并 DI 注册的 Mock adapter 与动态发现的插件 adapter。
- [ ] 明确短期 adapter 实例限制，后续再拆 descriptor/type factory。
- [ ] 验证开发环境下 `mock://` 可连接。

### Task 5: CLI Main Path And Nullable Cleanup

- [ ] 修复 `PgCommands.cs` 的 nullable 警告。
- [ ] 给主要 CLI 命令增加清晰异常输出。
- [ ] 运行 `dotnet build BertBridge.sln` 和 `dotnet run --project src/BertBridge.CLI -- device list`。

### Task 6: Minimal WPF MVVM Shell

- [ ] 添加 `CommunityToolkit.Mvvm`。
- [ ] 在 GUI 中建立 Generic Host 与 DI 启动。
- [ ] 创建 `MainViewModel` 和最小设备列表视图。
- [ ] 运行 `dotnet build src/BertBridge.GUI`。

### Task 7: Final Verification And Documentation

- [ ] 创建 `docs/architecture/plugin-adapter-flow.md`。
- [ ] 更新 `README.md`，记录 Mock 验证方式。
- [ ] 运行 `dotnet build BertBridge.sln`、`dotnet test`、Mock 连接和设备列表命令。
- [ ] 检查 `git status --short`，确认只包含本次范围内变更。
