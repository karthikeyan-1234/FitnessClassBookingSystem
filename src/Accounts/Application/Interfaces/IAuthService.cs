using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.DTOs;

using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginAsync(LoginRequest request);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<IEnumerable<User>> GetUsersByRoleAsync(Role role);
        Task<bool> UserExistsAsync(Guid id);
        Task<IEnumerable<User>> GetAllUsersAsync();

    }
}
