using Microsoft.Extensions.Options;
using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Options;
using OpsMonitor.Api.Security;
using SqlSugar;

namespace OpsMonitor.Api.Infrastructure;

public interface IDbBootstrapper
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

public class DbBootstrapper : IDbBootstrapper
{
    private readonly ISqlSugarClient _db;
    private readonly SeedOptions _seedOptions;
    private readonly IPasswordHasher _passwordHasher;

    public DbBootstrapper(
        ISqlSugarClient db,
        IOptions<SeedOptions> seedOptions,
        IPasswordHasher passwordHasher)
    {
        _db = db;
        _seedOptions = seedOptions.Value;
        _passwordHasher = passwordHasher;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _db.Ado.ExecuteCommandAsync("PRAGMA journal_mode=WAL;");
        await _db.Ado.ExecuteCommandAsync("PRAGMA synchronous=NORMAL;");

        _db.CodeFirst.InitTables(
            typeof(SysUser),
            typeof(SysAuditLog),
            typeof(MonMonitor),
            typeof(MonTarget),
            typeof(MonPolicy),
            typeof(MonCheckResult),
            typeof(AlertEvent),
            typeof(NotifyChannel),
            typeof(NotifyLog));

        await _db.Ado.ExecuteCommandAsync("CREATE UNIQUE INDEX IF NOT EXISTS idx_sys_user_username ON sys_user(UserName);");
        await _db.Ado.ExecuteCommandAsync("CREATE INDEX IF NOT EXISTS idx_result_monitor_checkedat ON mon_check_result(MonitorId, CheckedAt);");
        await _db.Ado.ExecuteCommandAsync("CREATE INDEX IF NOT EXISTS idx_alert_dedup_state ON alert_event(DedupKey, State);");
        await EnsureColumnAsync("alert_event", "AckNote", "TEXT NULL");

        var admin = await _db.Queryable<SysUser>()
            .Where(x => x.UserName == _seedOptions.AdminUserName)
            .FirstAsync();

        if (admin is not null)
        {
            return;
        }

        var hashed = _passwordHasher.HashPassword(_seedOptions.AdminPassword);
        var now = DateTime.UtcNow;
        await _db.Insertable(new SysUser
        {
            UserName = _seedOptions.AdminUserName,
            DisplayName = "System Admin",
            Role = UserRole.Admin,
            PasswordHash = hashed.Hash,
            PasswordSalt = hashed.Salt,
            PwdIterations = hashed.Iterations,
            IsEnabled = true,
            RequirePasswordChange = true,
            CreatedAt = now,
            UpdatedAt = now,
            LastLoginAt = now
        }).ExecuteCommandAsync();
    }

    private async Task EnsureColumnAsync(string table, string column, string columnType)
    {
        var exists = await _db.Ado.SqlQueryAsync<SqliteColumnInfo>($"PRAGMA table_info({table});");
        if (exists.Any(x => x.name.Equals(column, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        await _db.Ado.ExecuteCommandAsync($"ALTER TABLE {table} ADD COLUMN {column} {columnType};");
    }

    private class SqliteColumnInfo
    {
        public int cid { get; set; }
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
    }
}
