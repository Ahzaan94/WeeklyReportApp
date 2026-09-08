namespace WeeklyReportApp.API.DTOs
{
    public record ProjectDto(int Id, string Name, string? Description, bool IsActive, DateTime CreatedAt);
    public record UpsertProjectDto(string Name, string? Description, bool IsActive);
    public record AssignUserToProjectDto(string UserId, int ProjectId);

    public record DashboardSummaryDto(
        int TotalReportsThisWeek,
        int SubmittedCount,
        int PendingCount,
        int LateCount,
        int NeedsCorrectionCount,
        int OpenBlockersCount
    );

    public record MemberWeekStatusDto(string UserId, string UserFullName, string Status); // Draft/Submitted/NeedsCorrection/Approved/NotStarted

    public record TaskTrendPointDto(string WeekLabel, int TasksCompleted);
    public record StatusByMemberDto(string UserFullName, string Status);
    public record WorkloadByProjectDto(string ProjectName, double TotalHours);
    public record TimeByTaskTypeDto(string TaskType, double TotalHours);

    public record ActivityFeedItemDto(
        string Description,
        DateTime Timestamp,
        string ActorName
    );

    public record SectionAcrossTeamDto(string UserFullName, List<string> Items); // e.g. all blockers for the week, per member
}
