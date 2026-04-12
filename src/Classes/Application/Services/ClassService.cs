using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.DTOs.Class;
using Application.Interfaces;

using Domain.Entities;
using Domain.Interfaces;

using Shared;

namespace Application.Services
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly IAccountAPIClient _accountApi;

        public ClassService(IClassRepository classRepo, IBookingRepository bookingRepo, IAccountAPIClient accountApi)
        {
            _classRepo = classRepo;
            _bookingRepo = bookingRepo;
            _accountApi = accountApi;
        }

        public async Task<PagedResult<ClassResponse>> GetClassesForMemberAsync(Guid memberId, int page, int pageSize)
        {
            var bookings = await _bookingRepo.GetByMemberAsync(memberId);
            var classIds = bookings.Select(b => b.ClassId).Distinct();

            var allClasses = new List<FitnessClass>();
            foreach (var id in classIds)
            {
                var c = await _classRepo.GetByIdAsync(id);
                if (c != null) allClasses.Add(c);
            }

            var total = allClasses.Count;
            var pagedClasses = allClasses.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var response = pagedClasses.Select(MapToClassResponse).ToList();
            return new PagedResult<ClassResponse>(response, total, page, pageSize);
        }

        public async Task<ClassResponse?> GetClassByIdAsync(Guid classId)
        {
            var c = await _classRepo.GetByIdAsync(classId);
            return c == null ? null : MapToClassResponse(c);
        }

        public async Task<ClassResponse> CreateClassAsync(CreateClassRequest request, Guid requestingUserId)
        {
            // Validate instructor exists in AccountAPI
            var instructorExists = await _accountApi.InstructorExistsAsync(request.InstructorId);
            if (!instructorExists)
                throw new DomainException($"Instructor with ID '{request.InstructorId}' does not exist");

            // Fetch instructor name for denormalization
            var instructorName = await _accountApi.GetInstructorNameAsync(request.InstructorId);
            if (string.IsNullOrEmpty(instructorName))
                throw new DomainException("Unable to retrieve instructor name");

            if (request.Capacity <= 0)
                throw new DomainException("Capacity must be greater than zero");

            var fitnessClass = new FitnessClass
            {
                Title = request.Title,
                Description = request.Description,
                InstructorId = request.InstructorId,
                InstructorName = instructorName,
                ScheduledAt = request.ScheduledAt,
                Capacity = request.Capacity,
                BookedCount = 0
            };

            await _classRepo.AddAsync(fitnessClass);
            await _classRepo.SaveChangesAsync();

            return MapToClassResponse(fitnessClass);
        }

        public async Task<ClassResponse?> UpdateClassAsync(Guid classId, UpdateClassRequest request, Guid requestingUserId)
        {
            var existing = await _classRepo.GetByIdAsync(classId);
            if (existing == null)
                return null;

            // Only the instructor who created the class can update it
            if (existing.InstructorId != requestingUserId)
                throw new DomainException("You are not authorized to update this class");

            if (request.Capacity < existing.BookedCount)
                throw new DomainException($"Cannot reduce capacity below current booked count ({existing.BookedCount})");

            existing.Title = request.Title;
            existing.Description = request.Description;
            existing.ScheduledAt = request.ScheduledAt;
            existing.Capacity = request.Capacity;

            await _classRepo.UpdateAsync(existing);
            await _classRepo.SaveChangesAsync();

            return MapToClassResponse(existing);
        }

        public async Task<bool> DeleteClassAsync(Guid classId, Guid requestingUserId)
        {
            var existing = await _classRepo.GetByIdAsync(classId);
            if (existing == null)
                return false;

            if (existing.InstructorId != requestingUserId)
                throw new DomainException("You are not authorized to delete this class");

            await _classRepo.DeleteAsync(classId);
            await _classRepo.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResult<ClassResponse>> SearchClassesAsync(ClassSearchRequest request)
        {
            var skip = (request.Page - 1) * request.PageSize;
            var allFiltered = await _classRepo.SearchAsync(request.Date, request.InstructorName);

            var total = allFiltered.Count();
            var paged = allFiltered.Skip(skip).Take(request.PageSize).ToList();

            var response = paged.Select(MapToClassResponse).ToList();
            return new PagedResult<ClassResponse>(response, total, request.Page, request.PageSize);
        }

        private ClassResponse MapToClassResponse(FitnessClass c)
        {
            return new ClassResponse
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                InstructorId = c.InstructorId,
                InstructorName = c.InstructorName,
                ScheduledAt = c.ScheduledAt,
                Capacity = c.Capacity,
                BookedCount = c.BookedCount
            };
        }
    }
}
