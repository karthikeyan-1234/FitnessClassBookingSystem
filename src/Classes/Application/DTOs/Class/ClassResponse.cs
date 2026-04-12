using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Class
{
    public class ClassResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public int Capacity { get; set; }
        public int BookedCount { get; set; }
        public int AvailableSlots => Capacity - BookedCount;
    }
}
