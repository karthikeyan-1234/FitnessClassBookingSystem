using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Entities;
using Domain.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ClassDbContext _context;

        public BookingRepository(ClassDbContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            return await _context.Bookings
                .Include(b => b.Class)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> ExistsAsync(Guid classId, Guid memberId)
        {
            return await _context.Bookings
                .AnyAsync(b => b.ClassId == classId && b.MemberId == memberId);
        }

        public async Task<IEnumerable<Booking>> GetByMemberAsync(Guid memberId)
        {
            return await _context.Bookings
                .Include(b => b.Class)
                .Where(b => b.MemberId == memberId)
                .OrderByDescending(b => b.BookedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
        }

        public async Task DeleteAsync(Guid bookingId)
        {
            var booking = await GetByIdAsync(bookingId);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
