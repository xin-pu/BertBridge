using Microsoft.EntityFrameworkCore;

namespace BertBridge.Infrastructure.Persistence;

public static class BertBridgeDbContextSchemaExtensions
{
    public static async Task EnsureSchemaAsync(this BertBridgeDbContext dbContext, CancellationToken ct = default)
    {
        await dbContext.Database.EnsureCreatedAsync(ct);

        if (!dbContext.Database.IsSqlite())
            return;

        var deviceColumns = await GetColumnsAsync(dbContext, "Devices", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "InfoModel", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "InfoSerialNumber", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "InfoFirmwareVersion", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "InfoBoardType", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "ConnectionValue", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "ConnectionProtocol", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "CapabilityMaxLanes", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "CapabilitySupportsPAM4", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "CapabilitySupportsAdvancedModulation", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "CapabilitySupportedPatternsJson", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "CapabilityMaxBaudRateGBd", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "CapabilitySupportsFec", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "CapabilitySupportsGpio", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "CapabilityFirTapCount", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, "Devices", deviceColumns, "CapabilitySupportsJitterInjection", "INTEGER", ct);

        var sessionColumns = await GetColumnsAsync(dbContext, "TestSessions", ct);
        await AddColumnIfMissingAsync(dbContext, "TestSessions", sessionColumns, "ConfigurationDeviceId", "TEXT NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'", ct);
        await AddColumnIfMissingAsync(dbContext, "TestSessions", sessionColumns, "ConfigurationLaneCount", "INTEGER NOT NULL DEFAULT 1", ct);
        await AddColumnIfMissingAsync(dbContext, "TestSessions", sessionColumns, "ConfigurationPatternsJson", "TEXT NOT NULL DEFAULT '{}'", ct);
        await AddColumnIfMissingAsync(dbContext, "TestSessions", sessionColumns, "ConfigurationDuration", "TEXT", ct);
        await AddColumnIfMissingAsync(dbContext, "TestSessions", sessionColumns, "ConfigurationSnapshotTime", "TEXT NOT NULL DEFAULT '1970-01-01T00:00:00.0000000Z'", ct);
        await AddColumnIfMissingAsync(dbContext, "TestSessions", sessionColumns, "SummaryBerMantissa", "REAL", ct);
        await AddColumnIfMissingAsync(dbContext, "TestSessions", sessionColumns, "SummaryBerExponent", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, "TestSessions", sessionColumns, "SummaryBerErrorCount", "INTEGER", ct);
        await AddColumnIfMissingAsync(dbContext, "TestSessions", sessionColumns, "SummaryBerTotalCount", "INTEGER", ct);
    }

    private static async Task<HashSet<string>> GetColumnsAsync(
        BertBridgeDbContext dbContext,
        string tableName,
        CancellationToken ct)
    {
        var connection = dbContext.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{tableName}');";

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(ct);

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static async Task AddColumnIfMissingAsync(
        BertBridgeDbContext dbContext,
        string tableName,
        HashSet<string> existingColumns,
        string columnName,
        string columnType,
        CancellationToken ct)
    {
        if (existingColumns.Contains(columnName))
            return;

        var connection = dbContext.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnType};";

        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(ct);

        await command.ExecuteNonQueryAsync(ct);
        existingColumns.Add(columnName);
    }
}
