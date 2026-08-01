namespace BertBridge.Domain.TestSession;

/// <summary>
/// TestSession 仓储接口。
/// </summary>
public interface ITestSessionRepository
{
    Task<TestSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TestSession>> GetByDeviceIdAsync(Guid deviceId, CancellationToken ct = default);
    Task<IReadOnlyList<TestSession>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(TestSession session, CancellationToken ct = default);
    Task UpdateAsync(TestSession session, CancellationToken ct = default);
    Task AddBerDataPointAsync(BerDataPoint point, CancellationToken ct = default);
}
