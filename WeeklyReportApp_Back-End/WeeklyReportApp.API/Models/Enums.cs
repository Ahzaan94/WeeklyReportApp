namespace WeeklyReportApp.API.Models
{
    // Fixed set of roles required by the assignment
    public static class Roles
    {
        public const string TeamMember = "TeamMember";
        public const string Manager = "Manager";
    }

    public enum ReportStatus
    {
        Draft = 0,
        Submitted = 1,
        NeedsCorrection = 2,
        Approved = 3
    }

    public enum TaskStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Blocked = 3
    }

    public enum TaskType
    {
        Development = 0,
        Testing = 1,
        Meetings = 2,
        Documentation = 3,
        Other = 4
    }
}
