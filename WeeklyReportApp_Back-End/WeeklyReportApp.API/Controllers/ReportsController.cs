using System.Security.Claims;
using System.Text.Json;
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
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        private bool IsManager => User.IsInRole(Roles.Manager);

        // ---------- CREATE (Draft) ----------
        [HttpPost]
        [Authorize(Roles = Roles.TeamMember)]
        public async Task<ActionResult<ReportDetailDto>> Create(UpsertReportDto dto)
        {
            var exists = await _db.WeeklyReports.AnyAsync(r =>
                r.UserId == CurrentUserId && r.ProjectId == dto.ProjectId && r.WeekStartDate == dto.WeekStartDate);
            if (exists) return Conflict("A report for this project/week already exists.");

            var report = new WeeklyReport
            {
                UserId = CurrentUserId,
                ProjectId = dto.ProjectId,
                WeekStartDate = dto.WeekStartDate,
                WeekEndDate = dto.WeekEndDate,
                Status = ReportStatus.Draft,
                PlannedNextWeek = dto.PlannedNextWeek,
                Notes = dto.Notes,
                Tasks = dto.Tasks.Select(MapTask).ToList(),
                Blockers = dto.Blockers.Select(MapBlocker).ToList(),
                Achievements = dto.Achievements.Select(MapAchievement).ToList(),
                HoursByType = (dto.HoursByType ?? new()).Select(MapHours).ToList()
            };

            _db.WeeklyReports.Add(report);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = report.Id }, await BuildDetailDto(report.Id));
        }

        // ---------- EDIT (only while Draft or NeedsCorrection, owner only) ----------
        [HttpPut("{id}")]
        [Authorize(Roles = Roles.TeamMember)]
        public async Task<ActionResult<ReportDetailDto>> Update(int id, UpsertReportDto dto)
        {
            var report = await LoadFullReport(id);
            if (report == null) return NotFound();
            if (report.UserId != CurrentUserId) return Forbid();
            if (report.Status != ReportStatus.Draft && report.Status != ReportStatus.NeedsCorrection)
                return BadRequest("Report can only be edited while Draft or Needs Correction.");

            // REQUIRED: if this report previously went through a correction cycle,
            // snapshot the current content as a version before overwriting it.
            if (report.Status == ReportStatus.NeedsCorrection)
            {
                await SnapshotVersion(report);
                report.CurrentVersionNumber += 1;
            }

            report.ProjectId = dto.ProjectId;
            report.WeekStartDate = dto.WeekStartDate;
            report.WeekEndDate = dto.WeekEndDate;
            report.PlannedNextWeek = dto.PlannedNextWeek;
            report.Notes = dto.Notes;
            report.UpdatedAt = DateTime.UtcNow;

            _db.ReportTaskItems.RemoveRange(report.Tasks);
            _db.Blockers.RemoveRange(report.Blockers);
            _db.Achievements.RemoveRange(report.Achievements);
            _db.HoursEntries.RemoveRange(report.HoursByType);

            report.Tasks = dto.Tasks.Select(MapTask).ToList();
            report.Blockers = dto.Blockers.Select(MapBlocker).ToList();
            report.Achievements = dto.Achievements.Select(MapAchievement).ToList();
            report.HoursByType = (dto.HoursByType ?? new()).Select(MapHours).ToList();

            await _db.SaveChangesAsync();
            return Ok(await BuildDetailDto(report.Id));
        }

        // ---------- SUBMIT ----------
        [HttpPost("{id}/submit")]
        [Authorize(Roles = Roles.TeamMember)]
        public async Task<IActionResult> Submit(int id)
        {
            var report = await _db.WeeklyReports.FindAsync(id);
            if (report == null) return NotFound();
            if (report.UserId != CurrentUserId) return Forbid();
            if (report.Status != ReportStatus.Draft && report.Status != ReportStatus.NeedsCorrection)
                return BadRequest("Only Draft or Needs Correction reports can be submitted.");

            report.Status = ReportStatus.Submitted;
            report.SubmittedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------- REVIEW: Approve / Request Changes (Manager only, content untouched) ----------
        [HttpPost("{id}/review")]
        [Authorize(Roles = Roles.Manager)]
        public async Task<IActionResult> Review(int id, ReviewActionDto dto)
        {
            var report = await _db.WeeklyReports.FindAsync(id);
            if (report == null) return NotFound();
            if (report.Status != ReportStatus.Submitted)
                return BadRequest("Only Submitted reports can be reviewed.");

            report.Status = dto.Approve ? ReportStatus.Approved : ReportStatus.NeedsCorrection;
            report.LatestReviewerComment = dto.Comment;
            report.ReviewedByUserId = CurrentUserId;
            report.ReviewedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;

            _db.ReviewComments.Add(new ReviewComment
            {
                WeeklyReportId = report.Id,
                ReviewerId = CurrentUserId,
                Comment = dto.Comment ?? (dto.Approve ? "Approved." : string.Empty),
                ActionTaken = report.Status,
                AgainstVersionNumber = report.CurrentVersionNumber,
                CreatedAt = DateTime.UtcNow
            });

            if (!dto.Approve && string.IsNullOrWhiteSpace(dto.Comment))
                return BadRequest("A comment is required when requesting changes.");

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------- GET: my reports (team member) ----------
        [HttpGet("mine")]
        [Authorize(Roles = Roles.TeamMember)]
        public async Task<ActionResult<PagedResult<ReportListItemDto>>> GetMine(int page = 1, int pageSize = 20, ReportStatus? status = null)
        {
            var query = _db.WeeklyReports.Include(r => r.Project).Include(r => r.User)
                .Where(r => r.UserId == CurrentUserId);

            if (status.HasValue) query = query.Where(r => r.Status == status);

            var total = await query.CountAsync();
            var items = await query.OrderByDescending(r => r.WeekStartDate)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => MapListItem(r)).ToListAsync();

            return Ok(new PagedResult<ReportListItemDto>(items, total, page, pageSize));
        }

        // ---------- GET: all reports (manager dashboard), with filters ----------
        [HttpGet]
        [Authorize(Roles = Roles.Manager)]
        public async Task<ActionResult<PagedResult<ReportListItemDto>>> GetAll(
            int page = 1, int pageSize = 20,
            string? userId = null, int? projectId = null,
            DateTime? fromDate = null, DateTime? toDate = null,
            ReportStatus? status = null)
        {
            var query = _db.WeeklyReports.Include(r => r.Project).Include(r => r.User).AsQueryable();

            if (!string.IsNullOrEmpty(userId)) query = query.Where(r => r.UserId == userId);
            if (projectId.HasValue) query = query.Where(r => r.ProjectId == projectId);
            if (fromDate.HasValue) query = query.Where(r => r.WeekStartDate >= fromDate);
            if (toDate.HasValue) query = query.Where(r => r.WeekEndDate <= toDate);
            if (status.HasValue) query = query.Where(r => r.Status == status);

            var total = await query.CountAsync();
            var items = await query.OrderByDescending(r => r.WeekStartDate)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(r => MapListItem(r)).ToListAsync();

            return Ok(new PagedResult<ReportListItemDto>(items, total, page, pageSize));
        }

        // ---------- GET: single report detail (owner or manager) ----------
        [HttpGet("{id}")]
        public async Task<ActionResult<ReportDetailDto>> GetById(int id)
        {
            var report = await LoadFullReport(id);
            if (report == null) return NotFound();
            if (!IsManager && report.UserId != CurrentUserId) return Forbid();

            return Ok(await BuildDetailDto(id));
        }

        // ---------- Version history ----------
        [HttpGet("{id}/versions")]
        public async Task<ActionResult<List<ReportVersionSummaryDto>>> GetVersions(int id)
        {
            var report = await _db.WeeklyReports.FindAsync(id);
            if (report == null) return NotFound();
            if (!IsManager && report.UserId != CurrentUserId) return Forbid();

            var versions = await _db.ReportVersions.Where(v => v.WeeklyReportId == id)
                .OrderBy(v => v.VersionNumber)
                .Select(v => new ReportVersionSummaryDto(v.VersionNumber, v.SubmittedAt))
                .ToListAsync();
            return Ok(versions);
        }

        [HttpGet("{id}/versions/{versionNumber}")]
        public async Task<ActionResult<ReportVersionDetailDto>> GetVersion(int id, int versionNumber)
        {
            var report = await _db.WeeklyReports.FindAsync(id);
            if (report == null) return NotFound();
            if (!IsManager && report.UserId != CurrentUserId) return Forbid();

            var version = await _db.ReportVersions.FirstOrDefaultAsync(v => v.WeeklyReportId == id && v.VersionNumber == versionNumber);
            if (version == null) return NotFound();

            return Ok(new ReportVersionDetailDto(version.VersionNumber, version.SubmittedAt, version.ContentSnapshotJson));
        }

        // ---------- Review comment history (bonus) ----------
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<List<ReviewCommentDto>>> GetComments(int id)
        {
            var report = await _db.WeeklyReports.FindAsync(id);
            if (report == null) return NotFound();
            if (!IsManager && report.UserId != CurrentUserId) return Forbid();

            var comments = await _db.ReviewComments.Include(c => c.Reviewer)
                .Where(c => c.WeeklyReportId == id).OrderBy(c => c.CreatedAt)
                .Select(c => new ReviewCommentDto(c.Id, c.Reviewer!.FullName, c.Comment, c.ActionTaken, c.AgainstVersionNumber, c.CreatedAt))
                .ToListAsync();
            return Ok(comments);
        }

        // ---------------- helpers ----------------

        private async Task<WeeklyReport?> LoadFullReport(int id) =>
            await _db.WeeklyReports
                .Include(r => r.Tasks).Include(r => r.Blockers).Include(r => r.Achievements)
                .Include(r => r.HoursByType).Include(r => r.Project).Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

        private async Task<ReportDetailDto> BuildDetailDto(int id)
        {
            var r = (await LoadFullReport(id))!;
            return new ReportDetailDto(
                r.Id, r.UserId, r.User!.FullName, r.ProjectId, r.Project!.Name,
                r.WeekStartDate, r.WeekEndDate, r.Status,
                r.Tasks.Select(t => new TaskItemDto(t.Id, t.TaskName, t.Priority, t.PlannedPercent, t.ActualPercent, t.Status, t.TimePlannedHours, t.TimeSpentHours, t.Deliverable)).ToList(),
                r.PlannedNextWeek,
                r.Blockers.Select(b => new BlockerDto(b.Id, b.Description, b.IsKeyIssue)).ToList(),
                r.Achievements.Select(a => new AchievementDto(a.Id, a.Description, a.IsKeyAchievement)).ToList(),
                r.HoursByType.Select(h => new HoursEntryDto(h.TaskType, h.Hours)).ToList(),
                r.Notes, r.LatestReviewerComment, r.CurrentVersionNumber,
                r.CreatedAt, r.UpdatedAt, r.SubmittedAt
            );
        }

        private static ReportListItemDto MapListItem(WeeklyReport r) => new(
            r.Id, r.UserId, r.User!.FullName, r.ProjectId, r.Project!.Name,
            r.WeekStartDate, r.WeekEndDate, r.Status, r.UpdatedAt, r.SubmittedAt, r.CurrentVersionNumber);

        private static ReportTaskItem MapTask(TaskItemDto t) => new()
        {
            TaskName = t.TaskName, Priority = t.Priority, PlannedPercent = t.PlannedPercent,
            ActualPercent = t.ActualPercent, Status = t.Status, TimePlannedHours = t.TimePlannedHours,
            TimeSpentHours = t.TimeSpentHours, Deliverable = t.Deliverable
        };
        private static Blocker MapBlocker(BlockerDto b) => new() { Description = b.Description, IsKeyIssue = b.IsKeyIssue };
        private static Achievement MapAchievement(AchievementDto a) => new() { Description = a.Description, IsKeyAchievement = a.IsKeyAchievement };
        private static HoursEntry MapHours(HoursEntryDto h) => new() { TaskType = h.TaskType, Hours = h.Hours };

        private async Task SnapshotVersion(WeeklyReport report)
        {
            var snapshot = new
            {
                report.ProjectId,
                report.WeekStartDate,
                report.WeekEndDate,
                report.PlannedNextWeek,
                report.Notes,
                Tasks = report.Tasks.Select(t => new { t.TaskName, t.Priority, t.PlannedPercent, t.ActualPercent, t.Status, t.TimePlannedHours, t.TimeSpentHours, t.Deliverable }),
                Blockers = report.Blockers.Select(b => new { b.Description, b.IsKeyIssue }),
                Achievements = report.Achievements.Select(a => new { a.Description, a.IsKeyAchievement }),
                Hours = report.HoursByType.Select(h => new { h.TaskType, h.Hours })
            };

            _db.ReportVersions.Add(new ReportVersion
            {
                WeeklyReportId = report.Id,
                VersionNumber = report.CurrentVersionNumber,
                ContentSnapshotJson = JsonSerializer.Serialize(snapshot),
                SubmittedAt = report.SubmittedAt ?? report.UpdatedAt
            });
            await Task.CompletedTask;
        }
    }
}
