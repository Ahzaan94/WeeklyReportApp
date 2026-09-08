namespace WeeklyReportApp.API.DTOs
{
    public record RegisterDto(string FullName, string Email, string Password, string Role);
    public record LoginDto(string Email, string Password);
    public record AuthResponseDto(string Token, string UserId, string FullName, string Email, string Role, DateTime ExpiresAt);

    public record UserSummaryDto(string Id, string FullName, string Email, string Role, bool IsActive, DateTime CreatedAt);
    public record UpdateUserRoleDto(string Role);
    public record SetActiveDto(bool IsActive);
}
