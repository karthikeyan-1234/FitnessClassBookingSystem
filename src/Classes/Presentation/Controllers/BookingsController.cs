using System.Security.Claims;

using Application.DTOs.Booking;
using Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    [Authorize(Roles = "Member")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // Books a class for the authenticated member.
        [HttpPost]
        [SwaggerOperation(Summary = "Use Member login to Book a class")]
        public async Task<ActionResult<BookingResponse>> BookClass([FromBody] BookingRequest request)
        {
            var memberId = GetCurrentMemberId();
            var booking = await _bookingService.BookClassAsync(memberId, request);
            return CreatedAtAction(nameof(GetBookingsForMember), new { id = booking.BookingId }, booking);
        }

        // Gets all bookings for the authenticated member.
        [HttpGet]
        [SwaggerOperation(Summary = "Use Member login to get bookings of the logged in Member")]
        public async Task<ActionResult<IEnumerable<BookingResponse>>> GetBookingsForMember()
        {
            var memberId = GetCurrentMemberId();
            var bookings = await _bookingService.GetBookingsForMemberAsync(memberId);
            return Ok(bookings);
        }

        // Cancels a booking by ID (only the booking owner can cancel).
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Use Member login to cancel a booking, of the logged in Member. Other members can't cancel a booking")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var memberId = GetCurrentMemberId();
            var cancelled = await _bookingService.CancelBookingAsync(id, memberId);
            if (!cancelled)
                return NotFound(new { message = $"Booking with ID '{id}' not found" });
            return NoContent();
        }


        #region Helper method
        private Guid GetCurrentMemberId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var memberId))
                throw new UnauthorizedAccessException("Member ID not found in token");
            return memberId;
        }

        #endregion
    }
}
