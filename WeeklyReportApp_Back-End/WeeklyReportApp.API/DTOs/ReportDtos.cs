using WeeklyReportApp.API.Models;

namespace WeeklyReportApp.API.DTOs
{
    public record TaskItemDto(
        int Id,
        string TaskName,
        string Priority,
        int PlannedPercent,
        int ActualPercent,
        global::WeeklyReportApp.API.Models.TaskStatus Status,
        double TimePlannedHours,
        double TimeSpentHours,
        string? Deliverable);

    public record BlockerDto(int Id, string Description, bool IsKeyIssue);
    public record AchievementDto(int Id, string Description, bool IsKeyAchievement);
    public record HoursEntryDto(TaskType TaskType, double Hours);

    // Used for both create and edit — this is the ONLY shape a team member can submit.
    public record UpsertReportDto(
        int ProjectId,
        DateTime WeekStartDate,
        DateTime WeekEndDate,
        List<TaskItemDto> Tasks,
        string? PlannedNextWeek,
        List<BlockerDto> Blockers,
        List<AchievementDto> Achievements,
        List<HoursEntryDto>? HoursByType,
        string? Notes
    );

    public record ReportListItemDto(
        int Id,
        string UserId,
        string UserFullName,
        int ProjectId,
        string ProjectName,
        DateTime WeekStartDate,
        DateTime WeekEndDate,
        ReportStatus Status,
        DateTime UpdatedAt,
        DateTime? SubmittedAt,
        int CurrentVersionNumber
    );

    public record ReportDetailDto(
        int Id,
        string UserId,
        string UserFullName,
        int ProjectId,
        string ProjectName,
        DateTime WeekStartDate,
        DateTime WeekEndDate,
        ReportStatus Status,
        List<TaskItemDto> Tasks,
        string? PlannedNextWeek,
        List<BlockerDto> Blockers,
        List<AchievementDto> Achievements,
        List<HoursEntryDto> HoursByType,
        string? Notes,
        string? LatestReviewerComment,
        int CurrentVersionNumber,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        DateTime? SubmittedAt
    );

    // Manager can ONLY submit these two fields when reviewing — never report content.
    public record ReviewActionDto(bool Approve, string? Comment);

    public record ReportVersionSummaryDto(int VersionNumber, DateTime SubmittedAt);
    public record ReportVersionDetailDto(int VersionNumber, DateTime SubmittedAt, string ContentSnapshotJson);

    public record ReviewCommentDto(int Id, string ReviewerName, string Comment, ReportStatus ActionTaken, int AgainstVersionNumber, DateTime CreatedAt);

    public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
}
