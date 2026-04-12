using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAccountAPIClient
    {
        Task<bool> InstructorExistsAsync(Guid instructorId);
        Task<string?> GetInstructorNameAsync(Guid instructorId);
    }
}
