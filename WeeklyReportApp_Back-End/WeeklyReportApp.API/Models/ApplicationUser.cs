using Microsoft.AspNetCore.Identity;

namespace WeeklyReportApp.API.Models
{
    // Extends ASP.NET Identity's user with the fields we need.
    // Role membership itself is handled by Identity's Role tables (see Roles.cs constants).
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public ICollection<WeeklyReport> Reports { get; set; } = new List<WeeklyReport>();
        public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    }
}
