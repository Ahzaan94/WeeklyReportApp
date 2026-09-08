using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WeeklyReportApp.API.DTOs;
using WeeklyReportApp.API.Models;
using WeeklyReportApp.API.Services;

namespace WeeklyReportApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        // Public self-registration. In production you may want to restrict role selection
        // to "TeamMember" only and have an admin promote to Manager via /api/users.
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            if (dto.Role != Roles.TeamMember && dto.Role != Roles.Manager)
                return BadRequest("Role must be TeamMember or Manager.");

            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null) return Conflict("Email already registered.");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            await _userManager.AddToRoleAsync(user, dto.Role);

            var (token, expiresAt) = _tokenService.GenerateToken(user, dto.Role);
            return Ok(new AuthResponseDto(token, user.Id, user.FullName, user.Email!, dto.Role, expiresAt));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !user.IsActive) return Unauthorized("Invalid credentials.");

            var passwordOk = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordOk) return Unauthorized("Invalid credentials.");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? Roles.TeamMember;

            var (token, expiresAt) = _tokenService.GenerateToken(user, role);
            return Ok(new AuthResponseDto(token, user.Id, user.FullName, user.Email!, role, expiresAt));
        }

        // --- Admin-only user management (Section 7: User management page) ---

        [HttpGet("/api/users")]
        [Authorize(Roles = Roles.Manager)]
        public async Task<ActionResult<List<UserSummaryDto>>> GetUsers()
        {
            var users = _userManager.Users.ToList();
            var result = new List<UserSummaryDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserSummaryDto(u.Id, u.FullName, u.Email!, roles.FirstOrDefault() ?? "", u.IsActive, u.CreatedAt));
            }
            return Ok(result);
        }

        [HttpPut("/api/users/{id}/role")]
        [Authorize(Roles = Roles.Manager)]
        public async Task<IActionResult> UpdateRole(string id, UpdateUserRoleDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, dto.Role);
            return NoContent();
        }

        [HttpPut("/api/users/{id}/active")]
        [Authorize(Roles = Roles.Manager)]
        public async Task<IActionResult> SetActive(string id, SetActiveDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            user.IsActive = dto.IsActive;
            await _userManager.UpdateAsync(user);
            return NoContent();
        }
    }
}
