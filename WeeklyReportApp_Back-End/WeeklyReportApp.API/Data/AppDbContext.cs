using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WeeklyReportApp.API.Models;

namespace WeeklyReportApp.API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<UserProject> UserProjects => Set<UserProject>();
        public DbSet<WeeklyReport> WeeklyReports => Set<WeeklyReport>();
        public DbSet<ReportTaskItem> ReportTaskItems => Set<ReportTaskItem>();
        public DbSet<Blocker> Blockers => Set<Blocker>();
        public DbSet<Achievement> Achievements => Set<Achievement>();
        public DbSet<HoursEntry> HoursEntries => Set<HoursEntry>();
        public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();
        public DbSet<ReportVersion> ReportVersions => Set<ReportVersion>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // WeeklyReport relationships
            builder.Entity<WeeklyReport>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WeeklyReport>()
                .HasOne(r => r.Project)
                .WithMany(p => p.Reports)
                .HasForeignKey(r => r.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WeeklyReport>()
                .HasMany(r => r.Tasks)
                .WithOne(t => t.WeeklyReport)
                .HasForeignKey(t => t.WeeklyReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WeeklyReport>()
                .HasMany(r => r.Blockers)
                .WithOne(b => b.WeeklyReport)
                .HasForeignKey(b => b.WeeklyReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WeeklyReport>()
                .HasMany(r => r.Achievements)
                .WithOne(a => a.WeeklyReport)
                .HasForeignKey(a => a.WeeklyReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WeeklyReport>()
                .HasMany(r => r.HoursByType)
                .WithOne(h => h.WeeklyReport)
                .HasForeignKey(h => h.WeeklyReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WeeklyReport>()
                .HasMany(r => r.ReviewComments)
                .WithOne(c => c.WeeklyReport)
                .HasForeignKey(c => c.WeeklyReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WeeklyReport>()
                .HasMany(r => r.Versions)
                .WithOne(v => v.WeeklyReport)
                .HasForeignKey(v => v.WeeklyReportId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReviewComment>()
                .HasOne(c => c.Reviewer)
                .WithMany()
                .HasForeignKey(c => c.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            // A user can only have one report per project per week (prevents duplicates)
            builder.Entity<WeeklyReport>()
                .HasIndex(r => new { r.UserId, r.ProjectId, r.WeekStartDate })
                .IsUnique();

            // UserProject many-to-many link
            builder.Entity<UserProject>()
                .HasIndex(up => new { up.UserId, up.ProjectId })
                .IsUnique();

            builder.Entity<UserProject>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProjects)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserProject>()
                .HasOne(up => up.Project)
                .WithMany(p => p.UserProjects)
                .HasForeignKey(up => up.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
