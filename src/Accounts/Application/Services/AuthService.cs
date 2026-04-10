using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.DTOs;
using Application.Interfaces;

using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

using Shared;


namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtService;

        public AuthService(IUserRepository userRepo, IPasswordHasher passwordHasher, IJwtTokenService jwtService)
        {
            _userRepo = userRepo;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existing = await _userRepo.GetByUsernameAsync(request.Username);
            if (existing != null)
                throw new DomainException("Username already taken");

            var user = new User
            {
                Username = request.Username,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = request.Role
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);
            return new AuthResponse
            {
                Token = token,
                Expiry = _jwtService.GetTokenExpiry(),
                Username = user.Username,
                Role = user.Role.ToString()!
            };
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetByUsernameAsync(request.Username);
            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                return null;

            var token = _jwtService.GenerateToken(user);
            return new AuthResponse
            {
                Token = token,
                Expiry = _jwtService.GetTokenExpiry(),
                Username = user.Username,
                Role = user.Role.ToString()!
            };
        }

        public Task<User?> GetUserByIdAsync(Guid id) => _userRepo.GetByIdAsync(id);

        public Task<IEnumerable<User>> GetUsersByRoleAsync(Role role) => _userRepo.GetByRoleAsync(role);

        public Task<bool> UserExistsAsync(Guid id) => _userRepo.ExistsAsync(id);

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepo.GetAllAsync();
        }
    }
}
