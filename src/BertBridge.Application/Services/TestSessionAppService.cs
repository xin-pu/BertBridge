using BertBridge.Application.Contracts;
using BertBridge.Application.Dtos;
using BertBridge.Domain.TestSession;

namespace BertBridge.Application.Services;

/// <summary>
/// 测试会话应用服务。
/// </summary>
public class TestSessionAppService : ITestSessionAppService
{
    private readonly ITestSessionRepository _sessionRepository;

    public TestSessionAppService(ITestSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<TestSessionDto> CreateSessionAsync(CreateTestSessionDto dto, CancellationToken ct = default)
    {
        var config = new Domain.TestSession.TestConfiguration(
            dto.DeviceId, dto.LaneCount, dto.PatternsJson, dto.Duration);

        var session = Domain.TestSession.TestSession.Create(dto.DeviceId, config);
        await _sessionRepository.AddAsync(session, ct);

        return MapToDto(session);
    }

    public async Task<TestSessionDto> StartSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("测试会话不存在。");

        session.Start();
        await _sessionRepository.UpdateAsync(session, ct);

        return MapToDto(session);
    }

    public async Task<TestSessionDto> CompleteSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("测试会话不存在。");

        session.Complete();
        await _sessionRepository.UpdateAsync(session, ct);

        return MapToDto(session);
    }

    public async Task<TestSessionDto> AbortSessionAsync(Guid sessionId, string reason, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("测试会话不存在。");

        session.Abort(reason);
        await _sessionRepository.UpdateAsync(session, ct);

        return MapToDto(session);
    }

    public async Task<TestSessionDto?> GetSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, ct);
        return session == null ? null : MapToDto(session);
    }

    public async Task<IReadOnlyList<TestSessionDto>> GetSessionsByDeviceAsync(Guid deviceId, CancellationToken ct = default)
    {
        var sessions = await _sessionRepository.GetByDeviceIdAsync(deviceId, ct);
        return sessions.Select(MapToDto).ToList();
    }

    private static TestSessionDto MapToDto(Domain.TestSession.TestSession session) => new(
        session.Id,
        session.DeviceId,
        session.Status.ToString(),
        session.StartedAt,
        session.CompletedAt,
        session.Duration,
        session.SummaryBer != null ? session.SummaryBer.Mantissa * Math.Pow(10, session.SummaryBer.Exponent) : null,
        session.Notes,
        session.DataPoints.Count
    );
}
