using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IClassRepository
    {
        Task<FitnessClass?> GetByIdAsync(Guid id);
        Task<IEnumerable<FitnessClass>> GetByInstructorAsync(Guid instructorId);
        Task<IEnumerable<FitnessClass>> SearchAsync(DateTime? date, string? instructorName);
        Task<IEnumerable<FitnessClass>> GetPagedAsync(int skip, int take);
        Task<int> GetTotalCountAsync();
        Task AddAsync(FitnessClass fitnessClass);
        Task UpdateAsync(FitnessClass fitnessClass);
        Task DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
