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
    public class RegistrationTests
    {

        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IJwtTokenService> _jwtServiceMock;

        public RegistrationTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _jwtServiceMock = new Mock<IJwtTokenService>();

        }

        void registerMocks(RegisterRequest request)
        {
            // Simulate that username does not exist
            _userRepoMock.Setup(repo => repo.GetByUsernameAsync(request.Username))
                .ReturnsAsync((User)null!);

            // Simulate hashed password
            _passwordHasherMock.Setup(hasher => hasher.Hash(request.Password)).Returns("HashedPassprwrd");

            //Simulate JWT generation
            _jwtServiceMock.Setup(jwt => jwt.GenerateToken(It.IsAny<User>())).Returns("SomeJWTToken");


            //Simulate JWT Token expiry
            _jwtServiceMock.Setup(jwt => jwt.GetTokenExpiry()).Returns(DateTime.UtcNow.AddMinutes(30));

        }

        [Fact]
        public async Task Register_WithValidData_ReturnsAuthResponse()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "john_doe",
                Password = "Test@123",
                Role = Role.Member
            };

            registerMocks(request);

            //Act i.e. Test AuthService's RegisterAsync
            AuthService _authService = new AuthService(_userRepoMock.Object,
                                _passwordHasherMock.Object,
                                _jwtServiceMock.Object);

            var result = await _authService.RegisterAsync(request);

            //Assert
            result.Should().NotBeNull();
            result.Token.Should().Be("SomeJWTToken");
            result.Username.Should().Be(request.Username);
            result.Role.Should().Be("Member");

        }


        [Fact]
        public async Task Register_WithDuplicateUsername_ThrowsDomainException()
        {

            // Arrange
            var request = new RegisterRequest
            {
                Username = "john_doe",
                Password = "Test@123",
                Role = Role.Member
            };

            var existingUser = new User
            {
                Id = System.Guid.NewGuid(),
                Username = "existing_user",
                PasswordHash = "someHash",
                Role = Role.Member
            };

            registerMocks(request);

            // Override to Simulate that username exists
            _userRepoMock.Setup(repo => repo.GetByUsernameAsync(request.Username))
                .ReturnsAsync(existingUser);

            AuthService _authService = new AuthService(_userRepoMock.Object,
                                _passwordHasherMock.Object,
                                _jwtServiceMock.Object);
            try
            {
                var result = await _authService.RegisterAsync(request);
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<DomainException>();
            }


        }

        [Fact]
        public async Task Register_WhenUsernameIsTaken_DoesNotCallAddAsync()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "john_doe",
                Password = "Test@123",
                Role = Role.Member
            };

            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                Username = "john_doe",
                PasswordHash = "someHash",
                Role = Role.Member
            };

            registerMocks(request);

            // Simulate that username already exists (returns a user, not null)
            _userRepoMock
                .Setup(repo => repo.GetByUsernameAsync(request.Username))
                .ReturnsAsync(existingUser);

            var authService = new AuthService(
                _userRepoMock.Object,
                _passwordHasherMock.Object,
                _jwtServiceMock.Object);

            // Act & Assert
            try
            {
                await authService.RegisterAsync(request);
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<DomainException>();
            }

            // Verify that AddAsync was never called
            _userRepoMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        }


        [Fact]
        public async Task Register_WhenCalled_HashesPassword()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "john_doe",
                Password = "Test@123",
                Role = Role.Member
            };

            registerMocks(request);

            // Simulate that username already exists (returns a user, not null)
            _userRepoMock
                .Setup(repo => repo.GetByUsernameAsync(request.Username))
                .ReturnsAsync((User)null!);


            var authService = new AuthService(
               _userRepoMock.Object,
               _passwordHasherMock.Object,
               _jwtServiceMock.Object);


            // Act & Assert

            await authService.RegisterAsync(request);

            _passwordHasherMock.Verify(repo => repo.Hash(request.Password), Times.Once);


        }



        [Fact]
        public async Task Register_WhenCalled_GeneratesJwtToken()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "john_doe",
                Password = "Test@123",
                Role = Role.Member
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = request.Password,
                Role = Role.Member
            };

            registerMocks(request);

            // Simulate that username already exists (returns a user, not null)
            _userRepoMock
                .Setup(repo => repo.GetByUsernameAsync(request.Username))
                .ReturnsAsync((User)null!);


            var authService = new AuthService(
               _userRepoMock.Object,
               _passwordHasherMock.Object,
               _jwtServiceMock.Object);


            // Act & Assert

            await authService.RegisterAsync(request);

            _jwtServiceMock.Verify(repo => repo.GenerateToken(It.IsAny<User>()), Times.Once);


        }

    }

}
