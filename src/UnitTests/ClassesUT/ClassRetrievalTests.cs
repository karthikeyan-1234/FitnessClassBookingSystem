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
    public class ClassRetrievalTests
    {
        private readonly Mock<IClassRepository> _classRepoMock;
        private readonly Mock<IBookingRepository> _bookingRepoMock;
        private readonly Mock<IAccountAPIClient> _accountApiMock;
        private readonly ClassService _classService;

        public ClassRetrievalTests()
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
        public async Task GetClassById_WhenClassExists_ReturnsClassResponse()
        {
            var classId = Guid.NewGuid();
            var instructorId = Guid.NewGuid();
            var instructorName = "yoga_master";
            var scheduledAt = DateTime.UtcNow.AddDays(3);

            var existingClass = new FitnessClass
            {
                Id = classId,
                Title = "Advanced Yoga",
                Description = "For experienced practitioners",
                InstructorId = instructorId,
                InstructorName = instructorName,
                ScheduledAt = scheduledAt,
                Capacity = 20,
                BookedCount = 5
            };

            _classRepoMock.Setup(x => x.GetByIdAsync(classId)).ReturnsAsync(existingClass);

            var result = await _classService.GetClassByIdAsync(classId);

            result.Should().BeOfType<ClassResponse>();
        }

        [Fact]
        public async Task GetClassById_WhenClassDoesNotExist_ReturnsNull()
        {
            var classId = Guid.NewGuid();

            _classRepoMock.Setup(x => x.GetByIdAsync(classId)).ReturnsAsync((FitnessClass)null!);

            var result = await _classService.GetClassByIdAsync(classId);

            result.Should().Be(null);

        }


    }
}
