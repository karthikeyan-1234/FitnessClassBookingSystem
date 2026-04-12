using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.DTOs.Booking;
using Application.Interfaces;

using Domain.Entities;
using Domain.Interfaces;

using Shared;

namespace Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IClassRepository _classRepo;
        private readonly IBookingRepository _bookingRepo;

        public BookingService(IClassRepository classRepo, IBookingRepository bookingRepo)
        {
            _classRepo = classRepo;
            _bookingRepo = bookingRepo;
        }

        public async Task<BookingResponse> BookClassAsync(Guid memberId, BookingRequest request)
        {
            var fitnessClass = await _classRepo.GetByIdAsync(request.ClassId);
            if (fitnessClass == null)
                throw new DomainException("Class not found");

            if (fitnessClass.BookedCount >= fitnessClass.Capacity)
                throw new DomainException("Class is already full");

            var alreadyBooked = await _bookingRepo.ExistsAsync(request.ClassId, memberId);
            if (alreadyBooked)
                throw new DomainException("You have already booked this class");

            var booking = new Booking
            {
                ClassId = request.ClassId,
                MemberId = memberId,
                BookedAt = DateTime.UtcNow
            };

            fitnessClass.BookedCount++;

            await _bookingRepo.AddAsync(booking);
            await _classRepo.UpdateAsync(fitnessClass);
            await _bookingRepo.SaveChangesAsync();

            return new BookingResponse
            {
                BookingId = booking.Id,
                ClassId = booking.ClassId,
                ClassTitle = fitnessClass.Title,
                MemberId = booking.MemberId,
                BookedAt = booking.BookedAt
            };
        }

        public async Task<IEnumerable<BookingResponse>> GetBookingsForMemberAsync(Guid memberId)
        {
            var bookings = await _bookingRepo.GetByMemberAsync(memberId);
            return bookings.Select(b => new BookingResponse
            {
                BookingId = b.Id,
                ClassId = b.ClassId,
                ClassTitle = b.Class?.Title ?? string.Empty,
                MemberId = b.MemberId,
                BookedAt = b.BookedAt
            });
        }

        public async Task<bool> CancelBookingAsync(Guid bookingId, Guid memberId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                return false;

            if (booking.MemberId != memberId)
                throw new DomainException("You are not authorized to cancel this booking");

            var fitnessClass = await _classRepo.GetByIdAsync(booking.ClassId);
            if (fitnessClass != null)
            {
                fitnessClass.BookedCount--;
                await _classRepo.UpdateAsync(fitnessClass);
            }

            await _bookingRepo.DeleteAsync(bookingId);
            await _bookingRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsAlreadyBookedAsync(Guid classId, Guid memberId)
        {
            return await _bookingRepo.ExistsAsync(classId, memberId);
        }
    }
}
