using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id);
        Task<bool> ExistsAsync(Guid classId, Guid memberId);
        Task<IEnumerable<Booking>> GetByMemberAsync(Guid memberId);
        Task AddAsync(Booking booking);
        Task DeleteAsync(Guid bookingId);
        Task SaveChangesAsync();
    }
}
