using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeeklyReportApp.API.Data;
using WeeklyReportApp.API.DTOs;
using WeeklyReportApp.API.Models;

namespace WeeklyReportApp.API.Controllers
{
    [ApiController]
    [Route("api/projects")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ProjectsController(AppDbContext db) => _db = db;

        // Any authenticated user can list active projects (needed to fill the report form dropdown)
        [HttpGet]
        public async Task<ActionResult<List<ProjectDto>>> GetAll([FromQuery] bool includeInactive = false)
        {
            var query = _db.Projects.AsQueryable();
            if (!includeInactive) query = query.Where(p => p.IsActive);
            var projects = await query.OrderBy(p => p.Name)
                .Select(p => new ProjectDto(p.Id, p.Name, p.Description, p.IsActive, p.CreatedAt))
                .ToListAsync();
            return Ok(projects);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Manager)]
        public async Task<ActionResult<ProjectDto>> Create(UpsertProjectDto dto)
        {
            var project = new Project { Name = dto.Name, Description = dto.Description, IsActive = dto.IsActive };
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
            return Ok(new ProjectDto(project.Id, project.Name, project.Description, project.IsActive, project.CreatedAt));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Manager)]
        public async Task<IActionResult> Update(int id, UpsertProjectDto dto)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project == null) return NotFound();
            project.Name = dto.Name;
            project.Description = dto.Description;
            project.IsActive = dto.IsActive;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Manager)]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _db.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var hasReports = await _db.WeeklyReports.AnyAsync(r => r.ProjectId == id);
            if (hasReports)
            {
                // Soft-delete to preserve historical report integrity
                project.IsActive = false;
                await _db.SaveChangesAsync();
                return Ok(new { message = "Project has existing reports — deactivated instead of deleted." });
            }

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Optional: assign team members to projects
        [HttpPost("assign")]
        [Authorize(Roles = Roles.Manager)]
        public async Task<IActionResult> AssignUser(AssignUserToProjectDto dto)
        {
            var already = await _db.UserProjects.AnyAsync(up => up.UserId == dto.UserId && up.ProjectId == dto.ProjectId);
            if (already) return Conflict("User already assigned to this project.");

            _db.UserProjects.Add(new UserProject { UserId = dto.UserId, ProjectId = dto.ProjectId });
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/members")]
        [Authorize(Roles = Roles.Manager)]
        public async Task<ActionResult<List<string>>> GetMembers(int id)
        {
            var memberIds = await _db.UserProjects.Where(up => up.ProjectId == id)
                .Select(up => up.UserId).ToListAsync();
            return Ok(memberIds);
        }
    }
}
