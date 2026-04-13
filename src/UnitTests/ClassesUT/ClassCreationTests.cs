using Application.DTOs.Class;
using Application.Interfaces;
using Application.Services;

using Domain.Entities;
using Domain.Interfaces;

using FluentAssertions;

using Moq;

using Shared;

namespace ClassesUT
{
    public class ClassCreationTests
    {
        private readonly Mock<IClassRepository> _classRepoMock;
        private readonly Mock<IBookingRepository> _bookingRepoMock;
        private readonly Mock<IAccountAPIClient> _accountApiMock;
        private readonly ClassService _classService;

        public ClassCreationTests()
        {
            _classRepoMock = new Mock<IClassRepository>();
            _bookingRepoMock = new Mock<IBookingRepository>();
            _accountApiMock = new Mock<IAccountAPIClient>();
            _classService = new ClassService(
                _classRepoMock.Object,
                _bookingRepoMock.Object,
                _accountApiMock.Object);
        }

        [Fact]
        public async Task CreateClass_WithValidData_ReturnsClassResponse()
        {
            // Arrange
            var request = new CreateClassRequest
            {
                Title = "Yoga for Beginners",
                Description = "Learn basic yoga poses",
                InstructorId = Guid.NewGuid(),
                ScheduledAt = DateTime.UtcNow.AddDays(3),
                Capacity = 15
            };


            var instructorName = "instructor1";

            _accountApiMock
                .Setup(api => api.InstructorExistsAsync(request.InstructorId))
                .ReturnsAsync(true);

            _accountApiMock
                .Setup(api => api.GetInstructorNameAsync(request.InstructorId))
                .ReturnsAsync(instructorName);


            _classRepoMock.Setup(cl => cl.AddAsync(It.IsAny<FitnessClass>())).Returns(Task.CompletedTask);
            _classRepoMock.Setup(cl => cl.SaveChangesAsync()).Returns(Task.CompletedTask);


            // Act
            var result = await _classService.CreateClassAsync(request, request.InstructorId);

            result.Should().NotBeNull();
            result.Should().BeOfType<ClassResponse>();
        }


        [Fact]
        public async Task CreateClass_WhenInstructorDoesNotExist_ThrowsDomainException()
        {
            // Arrange
            var request = new CreateClassRequest
            {
                Title = "Yoga for Beginners",
                Description = "Learn basic yoga poses",
                InstructorId = Guid.NewGuid(),
                ScheduledAt = DateTime.UtcNow.AddDays(3),
                Capacity = 15
            };

            _accountApiMock
                .Setup(api => api.InstructorExistsAsync(request.InstructorId))
                .ReturnsAsync(false);

            // Act
            try
            {
                var result = await _classService.CreateClassAsync(request, request.InstructorId);
            }
            catch (Exception ex)
            {

                //Assert
                ex.Should().BeOfType<DomainException>();

            }

        }

        [Fact]
        public async Task CreateClass_WhenCapacityIsZeroOrNegative_ThrowsDomainException()
        {
            // Arrange
            var request = new CreateClassRequest
            {
                Title = "Yoga for Beginners",
                Description = "Learn basic yoga poses",
                InstructorId = Guid.NewGuid(),
                ScheduledAt = DateTime.UtcNow.AddDays(3),
                Capacity = 0
            };

            var instructorName = "instructor1";

            _accountApiMock
                .Setup(api => api.InstructorExistsAsync(request.InstructorId))
                .ReturnsAsync(true);

            _accountApiMock
                .Setup(api => api.GetInstructorNameAsync(request.InstructorId))
                .ReturnsAsync(instructorName);

            // Act
            try
            {
                var result = await _classService.CreateClassAsync(request, request.InstructorId);
            }
            catch (Exception ex)
            {

                //Assert
                ex.Should().BeOfType<DomainException>();

            }
        }
    }
}
