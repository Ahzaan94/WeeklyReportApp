namespace WeeklyReportApp.API.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<WeeklyReport> Reports { get; set; } = new List<WeeklyReport>();
        public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    }

    // Optional: assign team members to projects (Section 5, optional feature)
    public class UserProject
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
    }
}
