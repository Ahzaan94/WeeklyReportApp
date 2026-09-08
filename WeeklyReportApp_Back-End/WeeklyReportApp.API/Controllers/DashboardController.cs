using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeeklyReportApp.API.Data;
using WeeklyReportApp.API.DTOs;
using WeeklyReportApp.API.Models;

namespace WeeklyReportApp.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize(Roles = Roles.Manager)]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ---------- Summary metrics ----------
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary(DateTime weekStartDate)
        {
            var weekEndDate = weekStartDate.AddDays(6);
            var reportsThisWeek = await _db.WeeklyReports
                .Where(r => r.WeekStartDate == weekStartDate)
                .ToListAsync();

            var totalMembers = await _userManager.Users.CountAsync(u => u.IsActive);
            var submitted = reportsThisWeek.Count(r => r.Status is ReportStatus.Submitted or ReportStatus.Approved);
            var needsCorrection = reportsThisWeek.Count(r => r.Status == ReportStatus.NeedsCorrection);
            var isWeekOver = weekEndDate < DateTime.UtcNow.Date;
            var late = isWeekOver ? Math.Max(0, totalMembers - reportsThisWeek.Count) : 0;
            var pending = totalMembers - submitted - needsCorrection - late;

            var openBlockers = await _db.Blockers
                .Include(b => b.WeeklyReport)
                .Where(b => b.WeeklyReport!.WeekStartDate == weekStartDate && b.WeeklyReport.Status != ReportStatus.Approved)
                .CountAsync();

            return Ok(new DashboardSummaryDto(
                reportsThisWeek.Count, submitted, Math.Max(0, pending), late, needsCorrection, openBlockers));
        }

        // ---------- Per-member status for a given week (Section 4 filter/track requirement) ----------
        [HttpGet("member-status")]
        public async Task<ActionResult<List<MemberWeekStatusDto>>> GetMemberStatus(DateTime weekStartDate)
        {
            var members = await _userManager.GetUsersInRoleAsync(Roles.TeamMember);
            var reports = await _db.WeeklyReports.Where(r => r.WeekStartDate == weekStartDate).ToListAsync();

            var result = members.Where(m => m.IsActive).Select(m =>
            {
                var report = reports.FirstOrDefault(r => r.UserId == m.Id);
                var status = report?.Status.ToString() ?? "NotStarted";
                return new MemberWeekStatusDto(m.Id, m.FullName, status);
            }).ToList();

            return Ok(result);
        }

        // ---------- Chart: tasks completed trend over time ----------
        [HttpGet("charts/tasks-trend")]
        public async Task<ActionResult<List<TaskTrendPointDto>>> GetTasksTrend(string? userId = null, int weeks = 8)
        {
            var cutoff = DateTime.UtcNow.AddDays(-7 * weeks);
            var query = _db.WeeklyReports.Include(r => r.Tasks)
                .Where(r => r.WeekStartDate >= cutoff);
            if (!string.IsNullOrEmpty(userId)) query = query.Where(r => r.UserId == userId);

            var data = await query.ToListAsync();
            var grouped = data.GroupBy(r => r.WeekStartDate)
                .OrderBy(g => g.Key)
                .Select(g => new TaskTrendPointDto(
                    g.Key.ToString("MMM dd"),
                    g.SelectMany(r => r.Tasks).Count(t => t.Status == Models.TaskStatus.Completed)))
                .ToList();

            return Ok(grouped);
        }

        // ---------- Chart: submission/approval status by team member ----------
        [HttpGet("charts/status-by-member")]
        public async Task<ActionResult<List<StatusByMemberDto>>> GetStatusByMember(DateTime weekStartDate)
        {
            var statuses = await GetMemberStatus(weekStartDate);
            var list = (statuses.Result as OkObjectResult)!.Value as List<MemberWeekStatusDto>;
            return Ok(list!.Select(s => new StatusByMemberDto(s.UserFullName, s.Status)).ToList());
        }

        // ---------- Chart: workload/task distribution by project ----------
        [HttpGet("charts/workload-by-project")]
        public async Task<ActionResult<List<WorkloadByProjectDto>>> GetWorkloadByProject(int weeks = 8)
        {
            var cutoff = DateTime.UtcNow.AddDays(-7 * weeks);
            var data = await _db.WeeklyReports.Include(r => r.Project).Include(r => r.Tasks)
                .Where(r => r.WeekStartDate >= cutoff)
                .ToListAsync();

            var grouped = data.GroupBy(r => r.Project!.Name)
                .Select(g => new WorkloadByProjectDto(g.Key, g.SelectMany(r => r.Tasks).Sum(t => t.TimeSpentHours)))
                .OrderByDescending(g => g.TotalHours)
                .ToList();

            return Ok(grouped);
        }

        // ---------- Chart: time spent by task type, team-wide ----------
        [HttpGet("charts/time-by-task-type")]
        public async Task<ActionResult<List<TimeByTaskTypeDto>>> GetTimeByTaskType(int weeks = 8)
        {
            var cutoff = DateTime.UtcNow.AddDays(-7 * weeks);
            var data = await _db.HoursEntries.Include(h => h.WeeklyReport)
                .Where(h => h.WeeklyReport!.WeekStartDate >= cutoff)
                .ToListAsync();

            var grouped = data.GroupBy(h => h.TaskType)
                .Select(g => new TimeByTaskTypeDto(g.Key.ToString(), g.Sum(h => h.Hours)))
                .ToList();

            return Ok(grouped);
        }

        // ---------- Recent activity feed ----------
        [HttpGet("activity")]
        public async Task<ActionResult<List<ActivityFeedItemDto>>> GetActivity(int take = 20)
        {
            var recentComments = await _db.ReviewComments.Include(c => c.Reviewer).Include(c => c.WeeklyReport)
                .OrderByDescending(c => c.CreatedAt).Take(take)
                .Select(c => new ActivityFeedItemDto(
                    c.ActionTaken == ReportStatus.Approved
                        ? $"Approved {c.WeeklyReport!.User!.FullName}'s report"
                        : $"Requested changes on {c.WeeklyReport!.User!.FullName}'s report",
                    c.CreatedAt, c.Reviewer!.FullName))
                .ToListAsync();

            return Ok(recentComments);
        }

        // ---------- Bonus: one section across the whole team for a given week ----------
        [HttpGet("section/{sectionName}")]
        public async Task<ActionResult<List<SectionAcrossTeamDto>>> GetSectionAcrossTeam(string sectionName, DateTime weekStartDate)
        {
            var reports = await _db.WeeklyReports
                .Include(r => r.User).Include(r => r.Blockers).Include(r => r.Achievements)
                .Where(r => r.WeekStartDate == weekStartDate)
                .ToListAsync();

            List<SectionAcrossTeamDto> result = sectionName.ToLower() switch
            {
                "blockers" => reports.Select(r => new SectionAcrossTeamDto(
                    r.User!.FullName, r.Blockers.Select(b => b.Description).ToList())).ToList(),
                "achievements" => reports.Select(r => new SectionAcrossTeamDto(
                    r.User!.FullName, r.Achievements.Select(a => a.Description).ToList())).ToList(),
                _ => new List<SectionAcrossTeamDto>()
            };

            return Ok(result);
        }
    }
}
