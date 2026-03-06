using System.Collections.Concurrent;
using OpsMonitor.Api.Contracts;
using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Security;
using SqlSugar;

namespace OpsMonitor.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<UserInfoDto?> GetMeAsync(long userId, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly ISqlSugarClient _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private static readonly ConcurrentDictionary<string, (int Count, DateTime LastFailAt)> Failures = new();

    public AuthService(ISqlSugarClient db, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var key = request.UserName.Trim().ToLowerInvariant();
        if (Failures.TryGetValue(key, out var fail) &&
            fail.Count >= 5 &&
            DateTime.UtcNow - fail.LastFailAt < TimeSpan.FromMinutes(5))
        {
            return null;
        }

        var user = await _db.Queryable<SysUser>()
            .Where(x => x.UserName == request.UserName)
            .FirstAsync();

        if (user is null || !user.IsEnabled)
        {
            RegisterFailure(key);
            return null;
        }

        var ok = _passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt, user.PwdIterations);
        if (!ok)
        {
            RegisterFailure(key);
            return null;
        }

        Failures.TryRemove(key, out _);
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(user).ExecuteCommandAsync();

        var token = _jwtTokenService.CreateToken(user);
        return new LoginResponse(
            token.Token,
            token.ExpiresAt,
            new UserInfoDto(user.Id, user.UserName, user.DisplayName, user.Role, user.RequirePasswordChange));
    }

    public async Task<UserInfoDto?> GetMeAsync(long userId, CancellationToken ct = default)
    {
        var user = await _db.Queryable<SysUser>().InSingleAsync(userId);
        if (user is null || !user.IsEnabled)
        {
            return null;
        }
        return new UserInfoDto(user.Id, user.UserName, user.DisplayName, user.Role, user.RequirePasswordChange);
    }

    public async Task<bool> ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            throw new ArgumentException("New password must be at least 8 characters.");
        }

        var user = await _db.Queryable<SysUser>().InSingleAsync(userId);
        if (user is null || !user.IsEnabled)
        {
            return false;
        }

        var ok = _passwordHasher.Verify(
            request.CurrentPassword,
            user.PasswordHash,
            user.PasswordSalt,
            user.PwdIterations);
        if (!ok)
        {
            return false;
        }

        var hashed = _passwordHasher.HashPassword(request.NewPassword);
        user.PasswordHash = hashed.Hash;
        user.PasswordSalt = hashed.Salt;
        user.PwdIterations = hashed.Iterations;
        user.RequirePasswordChange = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(user).ExecuteCommandAsync();
        return true;
    }

    private static void RegisterFailure(string key)
    {
        Failures.AddOrUpdate(key, _ => (1, DateTime.UtcNow), (_, val) => (val.Count + 1, DateTime.UtcNow));
    }
}
