using OpsMonitor.Api.Contracts;
using OpsMonitor.Api.Domain;
using OpsMonitor.Api.Security;
using SqlSugar;

namespace OpsMonitor.Api.Services;

public interface IUserService
{
    Task<List<UserInfoDto>> GetAllAsync(CancellationToken ct = default);
    Task<long> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<bool> UpdateAsync(long id, UpdateUserRequest request, CancellationToken ct = default);
}

public class UserService : IUserService
{
    private readonly ISqlSugarClient _db;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(ISqlSugarClient db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserInfoDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _db.Queryable<SysUser>().OrderBy(x => x.Id).ToListAsync();
        return users.Select(x => new UserInfoDto(x.Id, x.UserName, x.DisplayName, x.Role, x.RequirePasswordChange)).ToList();
    }

    public async Task<long> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("UserName and Password are required.");
        }
        if (request.Role is not (UserRole.Admin or UserRole.User))
        {
            throw new ArgumentException("Role must be Admin or User.");
        }

        var existing = await _db.Queryable<SysUser>().AnyAsync(x => x.UserName == request.UserName);
        if (existing)
        {
            throw new ArgumentException("UserName already exists.");
        }

        var now = DateTime.UtcNow;
        var hashed = _passwordHasher.HashPassword(request.Password);
        var id = await _db.Insertable(new SysUser
        {
            UserName = request.UserName.Trim(),
            DisplayName = request.DisplayName?.Trim() ?? request.UserName.Trim(),
            Role = request.Role,
            PasswordHash = hashed.Hash,
            PasswordSalt = hashed.Salt,
            PwdIterations = hashed.Iterations,
            IsEnabled = true,
            RequirePasswordChange = false,
            CreatedAt = now,
            UpdatedAt = now
        }).ExecuteReturnIdentityAsync();

        return id;
    }

    public async Task<bool> UpdateAsync(long id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _db.Queryable<SysUser>().InSingleAsync(id);
        if (user is null)
        {
            return false;
        }
        if (request.Role is not (UserRole.Admin or UserRole.User))
        {
            throw new ArgumentException("Role must be Admin or User.");
        }

        user.DisplayName = request.DisplayName?.Trim() ?? user.DisplayName;
        user.Role = request.Role;
        user.IsEnabled = request.IsEnabled;
        user.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var hashed = _passwordHasher.HashPassword(request.NewPassword);
            user.PasswordHash = hashed.Hash;
            user.PasswordSalt = hashed.Salt;
            user.PwdIterations = hashed.Iterations;
            user.RequirePasswordChange = false;
        }
        await _db.Updateable(user).ExecuteCommandAsync();
        return true;
    }
}
