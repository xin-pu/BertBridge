namespace BertBridge.Domain.TestSession;

/// <summary>
/// 测试状态枚举。
/// </summary>
public enum TestStatus
{
    /// <summary>空闲/未开始</summary>
    Idle,

    /// <summary>正在运行</summary>
    Running,

    /// <summary>已完成</summary>
    Completed,

    /// <summary>已中止</summary>
    Aborted
}
