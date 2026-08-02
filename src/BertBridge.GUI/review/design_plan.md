# BertBridge WPF GUI 设计方案

## 审查目标
请 Codex 审查以下设计方案，给出具体的技术建议和改进点。

---

## 1. 整体布局设计

参考商用 BERT 仪器（Keysight M8040A, Anritsu MP1900A）和旧版 Fintest800GBert，采用如下布局：

```
┌──────────────────────────────────────────────────────────────┐
│  TitleBar: BertBridge │ Theme Toggle │ 时钟/日期             │
├────────────┬─────────────────────────────────────────────────┤
│ Navigation │  Content Area (可滚动)                           │
│ ────────── │                                                 │
│ 📊 仪表盘  │  ┌─────────────────────────────────────────┐    │
│ 📡 设备    │  │  当前选中的页面内容                      │    │
│ 🔬 通道BER │  │  (根据左侧 Nav 切换)                     │    │
│ 📈 FEC     │  └─────────────────────────────────────────┘    │
│ ⚙️ 设置    │                                                 │
├────────────┴─────────────────────────────────────────────────┤
│  StatusBar: 连接状态 │ BER 实时摘要 │ 全局控制按钮           │
└──────────────────────────────────────────────────────────────┘
```

- 左侧导航栏宽度 200px（可折叠到 50px 图标模式）
- 内容区域自适应剩余空间
- 底部状态栏 32px 高度

## 2. 主题切换

使用 DevExpress ThemeManager，支持以下主题：
- Win11Dark (默认)
- Win11Light
- Win10Dark
- Office2019Colorful
- Office2019Black

主题切换在顶部 TitleBar 通过下拉菜单或 ToggleButton 实现。

## 3. 各页面设计

### 3.1 Dashboard 仪表盘
- 顶部：设备状态概览卡片（已连接/总数、测试运行中、告警数）
- 中部：通道健康状态网格（8 通道 x 状态指示灯，每个通道显示 PG/ED 状态 + BER 级别）
- 底部：最近测试结果摘要

### 3.2 设备管理
- 设备列表（DataGrid / TileView）
- 连接/断开操作面板
- 设备信息详情（固件版本、序列号、通道数）

### 3.3 通道 BER 监控
- 左侧：通道列表（8 通道，每通道显示 PG Enable/ED Enable 开关）
- 右侧：选中通道的详细面板
  - PG 设置（Pattern 选择、Fir Taps、Swing 等）
  - ED 结果（Error Count, Total Count, BER, SNR）
  - 状态指示灯（Signal Detected, CDR Locked, PLL Locked 等）

### 3.4 FEC 统计
- 按 Chip 分组显示 FEC 统计
- Pre-FEC BER / Post-FEC BER 对比
- 可纠错/不可纠错码字统计

### 3.5 设置
- 主题选择
- 数据刷新间隔
- 日志级别

## 4. 技术方案

### 4.1 依赖包
- DevExpress.Wpf 26.1.3 (meta)
- DevExpress.Wpf.Core 26.1.3
- DevExpress.Wpf.Controls 26.1.3
- DevExpress.Wpf.Docking 26.1.3
- DevExpress.Wpf.Grid 26.1.3
- DevExpress.Wpf.LayoutControl 26.1.3
- DevExpress.Wpf.Themes.Office2019Colorful 26.1.3
- DevExpress.Wpf.Themes.Win11Light 26.1.3
- DevExpress.Wpf.Gauges 26.1.3
- CommunityToolkit.Mvvm 8.4.2 (已有)

### 4.2 Mock 数据
创建 `MockDataService` 实现所有 AppService 接口，生成模拟数据：
- 2 个模拟设备（QSFP-DD800, OSFP800）
- 每设备 8 通道
- 随机 BER 数据更新（定时器模拟实时数据）
- 随机 FEC 统计数据

### 4.3 ViewModel 设计
- `MainViewModel`：主导航 + 子 ViewModel 切换
- `DashboardViewModel`：仪表盘数据聚合
- `DeviceViewModel`：设备列表 + 连接操作
- `LanesViewModel`：通道列表 + 选中通道的 PG/ED 详情
- `FecViewModel`：FEC 统计数据
- `SettingsViewModel`：设置项

### 4.4 页面导航
使用 ContentControl + DataTemplate 方式：
- MainViewModel 暴露 `CurrentView` 属性
- 根据导航选择切换 ContentControl 内容
- 使用 DataTemplate 将 ViewModel 映射到 View

## 5. 待 Codex 审查的问题

1. DevExpress 26.1.3 对 .NET 10.0-windows 的兼容性评估
2. 是否应该使用 DevExpress Docking 替代静态导航布局
3. 仪表盘是否应该使用 DevExpress Gauges 控件
4. 是否有更好的 Mock 数据设计方案（如使用 Bogus 库生成真实数据）
5. 导航方案是否为最佳实践（ContentControl+DataTemplate vs Frame+Page vs PRISM）
6. 布局是否需要响应式设计？还是固定最小窗口尺寸
7. 对颜色方案和间距的 UX 建议
