using BertBridge.Domain.TestSession;
using Microsoft.EntityFrameworkCore;

namespace BertBridge.Infrastructure.Persistence.Repositories;

/// <summary>
/// TestSession 仓储实现。
/// </summary>
public class TestSessionRepository : ITestSessionRepository
{
    private readonly BertBridgeDbContext _context;

    public TestSessionRepository(BertBridgeDbContext context)
    {
        _context = context;
    }

    public async Task<TestSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.TestSessions
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<TestSession>> GetByDeviceIdAsync(Guid deviceId, CancellationToken ct = default)
        => await _context.TestSessions
            .Where(s => s.DeviceId == deviceId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TestSession>> GetAllAsync(CancellationToken ct = default)
        => await _context.TestSessions
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(ct);

    public async Task AddAsync(TestSession session, CancellationToken ct = default)
    {
        await _context.TestSessions.AddAsync(session, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TestSession session, CancellationToken ct = default)
    {
        _context.TestSessions.Update(session);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddBerDataPointAsync(BerDataPoint point, CancellationToken ct = default)
    {
        await _context.BerDataPoints.AddAsync(point, ct);
        await _context.SaveChangesAsync(ct);
    }
}
