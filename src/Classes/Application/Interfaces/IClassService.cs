using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.DTOs.Class;

using Shared;

namespace Application.Interfaces
{
    public interface IClassService
    {
        Task<PagedResult<ClassResponse>> GetClassesForMemberAsync(Guid memberId, int page, int pageSize);
        Task<ClassResponse?> GetClassByIdAsync(Guid classId);
        Task<ClassResponse> CreateClassAsync(CreateClassRequest request, Guid requestingUserId);
        Task<ClassResponse?> UpdateClassAsync(Guid classId, UpdateClassRequest request, Guid requestingUserId);
        Task<bool> DeleteClassAsync(Guid classId, Guid requestingUserId);
        Task<PagedResult<ClassResponse>> SearchClassesAsync(ClassSearchRequest request);
    }
}
