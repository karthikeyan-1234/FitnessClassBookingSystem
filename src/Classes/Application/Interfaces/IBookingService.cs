using Application.DTOs.Booking;

namespace Application.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponse> BookClassAsync(Guid memberId, BookingRequest request);
        Task<IEnumerable<BookingResponse>> GetBookingsForMemberAsync(Guid memberId);
        Task<bool> CancelBookingAsync(Guid bookingId, Guid memberId);
        Task<bool> IsAlreadyBookedAsync(Guid classId, Guid memberId);
    }
}