using Microsoft.AspNetCore.Identity;
using WeeklyReportApp.API.Models;

namespace WeeklyReportApp.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var db = services.GetRequiredService<AppDbContext>();

            // --- Roles ---
            foreach (var role in new[] { Roles.TeamMember, Roles.Manager })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // --- Manager ---
            var manager = await userManager.FindByEmailAsync("manager@demo.com");
            if (manager == null)
            {
                manager = new ApplicationUser { UserName = "manager@demo.com", Email = "manager@demo.com", FullName = "Alex Manager", EmailConfirmed = true };
                await userManager.CreateAsync(manager, "Passw0rd!");
                await userManager.AddToRoleAsync(manager, Roles.Manager);
            }

            // --- Team members ---
            var memberNames = new[] { "Priya Shah", "Daniel Kim", "Maria Gomez", "John Perera", "Wei Zhang" };
            var members = new List<ApplicationUser>();
            foreach (var name in memberNames)
            {
                var email = $"{name.Split(' ')[0].ToLower()}@demo.com";
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser { UserName = email, Email = email, FullName = name, EmailConfirmed = true };
                    await userManager.CreateAsync(user, "Passw0rd!");
                    await userManager.AddToRoleAsync(user, Roles.TeamMember);
                }
                members.Add(user);
            }

            // --- Projects ---
            if (!db.Projects.Any())
            {
                db.Projects.AddRange(
                    new Project { Name = "Client A", Description = "External client engagement" },
                    new Project { Name = "Internal Tooling", Description = "Internal dev tools" },
                    new Project { Name = "R&D", Description = "Research & prototyping" },
                    new Project { Name = "Marketing", Description = "Marketing site & campaigns" }
                );
                await db.SaveChangesAsync();
            }

            var projects = db.Projects.ToList();

            // --- Seeded reports across several weeks & statuses ---
            if (!db.WeeklyReports.Any())
            {
                var rnd = new Random(42);
                var statuses = new[] { ReportStatus.Draft, ReportStatus.Submitted, ReportStatus.NeedsCorrection, ReportStatus.Approved };
                var thisMonday = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek + 1);

                for (int weekOffset = 0; weekOffset < 4; weekOffset++)
                {
                    var weekStart = thisMonday.AddDays(-7 * weekOffset);
                    var weekEnd = weekStart.AddDays(6);

                    foreach (var member in members)
                    {
                        // Not every member has a report every week, to make "NotStarted" meaningful
                        if (rnd.NextDouble() < 0.15) continue;

                        var status = statuses[rnd.Next(statuses.Length)];
                        var project = projects[rnd.Next(projects.Count)];

                        var report = new WeeklyReport
                        {
                            UserId = member.Id,
                            ProjectId = project.Id,
                            WeekStartDate = weekStart,
                            WeekEndDate = weekEnd,
                            Status = status,
                            PlannedNextWeek = "Continue feature development and address open blockers.",
                            Notes = "Seeded demo report.",
                            SubmittedAt = status == ReportStatus.Draft ? null : weekStart.AddDays(4),
                            LatestReviewerComment = status == ReportStatus.NeedsCorrection ? "Please add more detail on the blocker impact and update task completion %." : null,
                            Tasks = new List<ReportTaskItem>
                            {
                                new() { TaskName = "Implement feature module", Priority = "High", PlannedPercent = 100, ActualPercent = rnd.Next(60, 101), Status = Models.TaskStatus.Completed, TimePlannedHours = 20, TimeSpentHours = rnd.Next(15,25), Deliverable = "PR merged" },
                                new() { TaskName = "Write unit tests", Priority = "Medium", PlannedPercent = 100, ActualPercent = rnd.Next(50, 100), Status = Models.TaskStatus.InProgress, TimePlannedHours = 8, TimeSpentHours = rnd.Next(4,9), Deliverable = "Test suite" }
                            },
                            Blockers = new List<Blocker>
                            {
                                new() { Description = "Waiting on API access from third-party vendor.", IsKeyIssue = true }
                            },
                            Achievements = new List<Achievement>
                            {
                                new() { Description = "Shipped the new dashboard widget ahead of schedule.", IsKeyAchievement = true }
                            },
                            HoursByType = new List<HoursEntry>
                            {
                                new() { TaskType = TaskType.Development, Hours = rnd.Next(15,25) },
                                new() { TaskType = TaskType.Meetings, Hours = rnd.Next(2,6) },
                                new() { TaskType = TaskType.Testing, Hours = rnd.Next(3,8) },
                                new() { TaskType = TaskType.Documentation, Hours = rnd.Next(1,4) }
                            }
                        };

                        db.WeeklyReports.Add(report);
                    }
                }

                await db.SaveChangesAsync();
            }
        }
    }
}
