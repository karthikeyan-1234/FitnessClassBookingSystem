using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Booking
{
    public class BookingResponse
    {
        public Guid BookingId { get; set; }
        public Guid ClassId { get; set; }
        public string ClassTitle { get; set; } = string.Empty;
        public Guid MemberId { get; set; }
        public DateTime BookedAt { get; set; }
    }
}
