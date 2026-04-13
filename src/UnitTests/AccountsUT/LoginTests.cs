using Application.DTOs;
using Application.Interfaces;
using Application.Services;

using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

using FluentAssertions;

using Moq;

using Shared;

namespace AccountsUT
{
    public class LoginTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IJwtTokenService> _jwtServiceMock;

        public LoginTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _jwtServiceMock = new Mock<IJwtTokenService>();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsAuthResponse()
        {
            //Arrange

            var request = new LoginRequest()
            {
                Username = "mail2karthikkn",
                Password = "Pass123"
            };

            var existingUser = new User()
            {
                Id = Guid.NewGuid(),
                Username = "john_doe",
                PasswordHash = "storedHash",
                Role = Role.Member
            };

            _userRepoMock.Setup(repo => repo.GetByUsernameAsync(request.Username)).ReturnsAsync(existingUser);
            _jwtServiceMock.Setup(jwt => jwt.GenerateToken(existingUser)).Returns("jwtToken");
            _jwtServiceMock.Setup(jwt => jwt.GetTokenExpiry()).Returns(DateTime.Now.AddMinutes(30));
            _passwordHasherMock.Setup(pwd => pwd.Verify(request.Password, existingUser.PasswordHash)).Returns(true);

            AuthService service = new AuthService(_userRepoMock.Object, _passwordHasherMock.Object, _jwtServiceMock.Object);
            var result = await service.LoginAsync(request);

            result.Should().BeOfType<AuthResponse>();
            result!.Token.Should().Be("jwtToken");
            result!.Username.Should().Be(existingUser.Username);
            result.Role.Should().Be("Member");
        }
    }
}
