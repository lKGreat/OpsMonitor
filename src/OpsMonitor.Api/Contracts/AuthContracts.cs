namespace OpsMonitor.Api.Contracts;

public record LoginRequest(string UserName, string Password);

public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAt,
    UserInfoDto User);

public record UserInfoDto(
    long Id,
    string UserName,
    string DisplayName,
    string Role,
    bool RequirePasswordChange);

public record CreateUserRequest(
    string UserName,
    string DisplayName,
    string Role,
    string Password);

public record UpdateUserRequest(
    string DisplayName,
    string Role,
    bool IsEnabled,
    string? NewPassword);
