using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClassId { get; set; }
        public Guid MemberId { get; set; }
        public DateTime BookedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public FitnessClass? Class { get; set; }
    }
}
