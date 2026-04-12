using System.Security.Claims;

using Application.DTOs.Class;
using Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Shared;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClassesController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassesController(IClassService classService)
        {
            _classService = classService;
        }        

        // Gets paginated list of classes booked by the authenticated member.
        [HttpGet]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<PagedResult<ClassResponse>>> GetMemberClasses(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var memberId = GetCurrentUserId();
            var result = await _classService.GetClassesForMemberAsync(memberId, page, pageSize);
            return Ok(result);
        }

        // Gets a single class by ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<ClassResponse>> GetClassById(Guid id)
        {
            var classResponse = await _classService.GetClassByIdAsync(id);
            if (classResponse == null)
                return NotFound(new { message = $"Class with ID '{id}' not found" });
            return Ok(classResponse);
        }

        // Searches classes by date and/or instructor name (paginated).
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<ClassResponse>>> SearchClasses(
            [FromQuery] DateTime? date,
            [FromQuery] string? instructorName,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var request = new ClassSearchRequest
            {
                Date = date,
                InstructorName = instructorName,
                Page = page,
                PageSize = pageSize
            };
            var result = await _classService.SearchClassesAsync(request);
            return Ok(result);
        }

        // Creates a new class (Instructor only).
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ClassResponse>> CreateClass([FromBody] CreateClassRequest request)
        {
            var instructorId = GetCurrentUserId();
            var created = await _classService.CreateClassAsync(request, instructorId);
            return CreatedAtAction(nameof(GetClassById), new { id = created.Id }, created);
        }

        // Updates an existing class (Instructor only, must be owner).
        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<ClassResponse>> UpdateClass(Guid id, [FromBody] UpdateClassRequest request)
        {
            var instructorId = GetCurrentUserId();
            var updated = await _classService.UpdateClassAsync(id, request, instructorId);
            if (updated == null)
                return NotFound(new { message = $"Class with ID '{id}' not found" });
            return Ok(updated);
        }

        // Deletes a class (Instructor only, must be owner).
        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteClass(Guid id)
        {
            var instructorId = GetCurrentUserId();
            var deleted = await _classService.DeleteClassAsync(id, instructorId);
            if (!deleted)
                return NotFound(new { message = $"Class with ID '{id}' not found" });
            return NoContent();
        }


        #region Helper method
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("User ID not found in token");
            return userId;
        }
        #endregion
    }
}
