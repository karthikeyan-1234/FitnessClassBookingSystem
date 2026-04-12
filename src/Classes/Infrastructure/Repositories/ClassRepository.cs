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
    public class ClassRepository : IClassRepository
    {
        private readonly ClassDbContext _context;

        public ClassRepository(ClassDbContext context)
        {
            _context = context;
        }

        public async Task<FitnessClass?> GetByIdAsync(Guid id)
        {
            return await _context.FitnessClasses
                .Include(c => c.Bookings)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<FitnessClass>> GetByInstructorAsync(Guid instructorId)
        {
            return await _context.FitnessClasses
                .Where(c => c.InstructorId == instructorId)
                .OrderBy(c => c.ScheduledAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<FitnessClass>> SearchAsync(DateTime? date, string? instructorName)
        {
            var query = _context.FitnessClasses.AsQueryable();

            if (date.HasValue)
            {
                query = query.Where(c => c.ScheduledAt.Date == date.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(instructorName))
            {
                query = query.Where(c => c.InstructorName.Contains(instructorName));
            }

            return await query.OrderBy(c => c.ScheduledAt).ToListAsync();
        }

        public async Task<IEnumerable<FitnessClass>> GetPagedAsync(int skip, int take)
        {
            return await _context.FitnessClasses
                .OrderBy(c => c.ScheduledAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.FitnessClasses.CountAsync();
        }

        public async Task AddAsync(FitnessClass fitnessClass)
        {
            await _context.FitnessClasses.AddAsync(fitnessClass);
        }

        public Task UpdateAsync(FitnessClass fitnessClass)
        {
            _context.FitnessClasses.Update(fitnessClass);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var fitnessClass = await GetByIdAsync(id);
            if (fitnessClass != null)
            {
                _context.FitnessClasses.Remove(fitnessClass);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
