using System.Security.Claims;

using Application.DTOs.Booking;
using Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    [Authorize(Policy = "MemberOnly")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        private Guid GetCurrentMemberId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var memberId))
                throw new UnauthorizedAccessException("Member ID not found in token");
            return memberId;
        }

        /// <summary>
        /// Books a class for the authenticated member.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BookingResponse>> BookClass([FromBody] BookingRequest request)
        {
            var memberId = GetCurrentMemberId();
            var booking = await _bookingService.BookClassAsync(memberId, request);
            return CreatedAtAction(nameof(GetBookingsForMember), new { id = booking.BookingId }, booking);
        }

        /// <summary>
        /// Gets all bookings for the authenticated member.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingResponse>>> GetBookingsForMember()
        {
            var memberId = GetCurrentMemberId();
            var bookings = await _bookingService.GetBookingsForMemberAsync(memberId);
            return Ok(bookings);
        }

        /// <summary>
        /// Cancels a booking by ID (only the booking owner can cancel).
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var memberId = GetCurrentMemberId();
            var cancelled = await _bookingService.CancelBookingAsync(id, memberId);
            if (!cancelled)
                return NotFound(new { message = $"Booking with ID '{id}' not found" });
            return NoContent();
        }
    }
}
