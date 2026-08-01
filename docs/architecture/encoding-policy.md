# 编码规范

BertBridge 仓库中的源码、XAML、JSON、Markdown、项目文件和解决方案文件统一使用 UTF-8。

## 规则

- 新文件遵守根目录 `.editorconfig`。
- 中文注释、中文 CLI 文案和中文文档必须能在 IDE 中正常显示。
- 如果 Windows PowerShell 读取 UTF-8 无 BOM 文件时显示乱码，先确认文件字节编码，不直接改写内容。
- 批量转换前必须检查 `git status --short`，避免覆盖用户未提交修改。
- 对已有乱码内容，应优先从版本历史或原始资料恢复语义，不猜测替换关键业务说明。

## 验证

常规验证命令：

```powershell
dotnet build BertBridge.sln
```

抽查建议：

- `src/BertBridge.PluginSDK/IDeviceAdapter.cs`
- `src/BertBridge.Application/Services/DeviceAppService.cs`
- `src/BertBridge.CLI/Commands/DeviceCommands.cs`

这些文件包含中文注释或中文 CLI 文案，适合作为编码显示检查样本。
