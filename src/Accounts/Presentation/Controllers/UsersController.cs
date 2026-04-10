using Application.Interfaces;

using Domain.Enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // All endpoints require authentication
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Get user details by ID (used by ClassAPI for instructor name lookup)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details (without password hash)</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = $"User with ID '{id}' not found" });

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Role
            });
        }

        /// <summary>
        /// Lightweight existence check (used by ClassAPI before assigning instructor to class)
        /// </summary>
        /// <param name="id">User ID to check</param>
        /// <returns>True if user exists, false otherwise</returns>
        [HttpGet("exists/{id}")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<bool>> Exists(Guid id)
        {
            var exists = await _authService.UserExistsAsync(id);
            return Ok(exists);
        }

        /// <summary>
        /// Get all instructors (used by ClassAPI to populate dropdowns)
        /// </summary>
        /// <returns>List of instructors with Id and Username</returns>
        [HttpGet("instructors")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetInstructors()
        {
            var instructors = await _authService.GetUsersByRoleAsync(Role.Instructor);
            return Ok(instructors.Select(i => new { i.Id, i.Username }));
        }


        /// <summary>
        /// Get all users (optional filtering by role)
        /// </summary>
        /// <param name="role">Optional role filter (Member or Instructor)</param>
        /// <returns>List of users (without password hash)</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? role = null)
        {
            IEnumerable<Domain.Entities.User> users;

            if (!string.IsNullOrEmpty(role) && Enum.TryParse<Role>(role, true, out var roleEnum))
            {
                users = await _authService.GetUsersByRoleAsync(roleEnum);
            }
            else
            {
                // If no role filter or invalid role, return all users
                // You may need to add a GetAllUsersAsync method to IAuthService and AuthService
                users = await _authService.GetAllUsersAsync();
            }

            return Ok(users.Select(u => new
            {
                u.Id,
                u.Username,
                u.Role
            }));
        }
    }
}
