namespace WeeklyReportApp.API.Models
{
    public class WeeklyReport
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }

        public ReportStatus Status { get; set; } = ReportStatus.Draft;

        // Latest reviewer comment (quick access — full history lives in ReviewComments)
        public string? LatestReviewerComment { get; set; }
        public string? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public string? PlannedNextWeek { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }

        // Increments every time the report is resubmitted after correction.
        // Used to tag which version a review comment was made against.
        public int CurrentVersionNumber { get; set; } = 1;

        public ICollection<ReportTaskItem> Tasks { get; set; } = new List<ReportTaskItem>();
        public ICollection<Blocker> Blockers { get; set; } = new List<Blocker>();
        public ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
        public ICollection<HoursEntry> HoursByType { get; set; } = new List<HoursEntry>();
        public ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();
        public ICollection<ReportVersion> Versions { get; set; } = new List<ReportVersion>();
    }

    public class ReportTaskItem
    {
        public int Id { get; set; }
        public int WeeklyReportId { get; set; }
        public WeeklyReport? WeeklyReport { get; set; }

        public string TaskName { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium"; // Low / Medium / High
        public int PlannedPercent { get; set; }
        public int ActualPercent { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.NotStarted;
        public double TimePlannedHours { get; set; }
        public double TimeSpentHours { get; set; }
        public string? Deliverable { get; set; }
    }

    public class Blocker
    {
        public int Id { get; set; }
        public int WeeklyReportId { get; set; }
        public WeeklyReport? WeeklyReport { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsKeyIssue { get; set; }
    }

    public class Achievement
    {
        public int Id { get; set; }
        public int WeeklyReportId { get; set; }
        public WeeklyReport? WeeklyReport { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsKeyAchievement { get; set; }
    }

    public class HoursEntry
    {
        public int Id { get; set; }
        public int WeeklyReportId { get; set; }
        public WeeklyReport? WeeklyReport { get; set; }
        public TaskType TaskType { get; set; }
        public double Hours { get; set; }
    }

    // Full history of review comments per report (bonus requirement)
    public class ReviewComment
    {
        public int Id { get; set; }
        public int WeeklyReportId { get; set; }
        public WeeklyReport? WeeklyReport { get; set; }
        public string ReviewerId { get; set; } = string.Empty;
        public ApplicationUser? Reviewer { get; set; }
        public string Comment { get; set; } = string.Empty;
        public ReportStatus ActionTaken { get; set; } // Approved or NeedsCorrection
        public int AgainstVersionNumber { get; set; } // which version this comment was made against
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Snapshot of report content taken right before an edit is applied after "Needs Correction".
    // Required by spec: past versions must remain viewable, not just overwritten.
    public class ReportVersion
    {
        public int Id { get; set; }
        public int WeeklyReportId { get; set; }
        public WeeklyReport? WeeklyReport { get; set; }
        public int VersionNumber { get; set; }
        public string ContentSnapshotJson { get; set; } = string.Empty; // serialized tasks/blockers/achievements/etc.
        public DateTime SubmittedAt { get; set; }
    }
}
