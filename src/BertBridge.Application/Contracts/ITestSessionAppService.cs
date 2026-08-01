using BertBridge.Application.Dtos;

namespace BertBridge.Application.Contracts;

/// <summary>
/// 测试会话应用服务接口。
/// </summary>
public interface ITestSessionAppService
{
    Task<TestSessionDto> CreateSessionAsync(CreateTestSessionDto dto, CancellationToken ct = default);
    Task<TestSessionDto> StartSessionAsync(Guid sessionId, CancellationToken ct = default);
    Task<TestSessionDto> CompleteSessionAsync(Guid sessionId, CancellationToken ct = default);
    Task<TestSessionDto> AbortSessionAsync(Guid sessionId, string reason, CancellationToken ct = default);
    Task<TestSessionDto?> GetSessionAsync(Guid sessionId, CancellationToken ct = default);
    Task<IReadOnlyList<TestSessionDto>> GetSessionsByDeviceAsync(Guid deviceId, CancellationToken ct = default);
}
