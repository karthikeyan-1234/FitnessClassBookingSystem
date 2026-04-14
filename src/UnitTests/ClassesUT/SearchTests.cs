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
    public class SearchTests
    {
        private readonly Mock<IClassRepository> _classRepoMock;
        private readonly Mock<IBookingRepository> _bookingRepoMock;
        private readonly Mock<IAccountAPIClient> _accountApiMock;
        private readonly ClassService _classService;

        public SearchTests()
        {
            _classRepoMock = new Mock<IClassRepository>();
            _bookingRepoMock = new Mock<IBookingRepository>();
            _accountApiMock = new Mock<IAccountAPIClient>();
            _classService = new ClassService(_classRepoMock.Object,_bookingRepoMock.Object,_accountApiMock.Object);
        }

        [Fact]
        public async Task SearchClasses_WithDateFilter_ReturnsFilteredClasses()
        {
            // Arrange
            var targetDate = new DateTime(2025, 5, 15);
            var allClasses = new List<FitnessClass>
            {
                new FitnessClass { Id = Guid.NewGuid(), ScheduledAt = targetDate, InstructorName = "YogaMaster", Title = "Morning Yoga" },
                new FitnessClass { Id = Guid.NewGuid(), ScheduledAt = targetDate.AddDays(1), InstructorName = "PilatesPro", Title = "Evening Pilates" },
                new FitnessClass { Id = Guid.NewGuid(), ScheduledAt = targetDate, InstructorName = "CardioCoach", Title = "HIIT Session" }
            };

            // Repository search should filter by date (the service will call SearchAsync with the filter)
            _classRepoMock.Setup(repo => repo.SearchAsync(targetDate, null)).ReturnsAsync(allClasses.Where(c => c.ScheduledAt.Date == targetDate.Date).ToList());

            var request = new ClassSearchRequest
            {
                Date = targetDate,
                InstructorName = null,
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _classService.SearchClassesAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items.All(c => c.ScheduledAt.Date == targetDate.Date).Should().BeTrue();
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task SearchClasses_WithInstructorNameFilter_ReturnsMatchingClasses()
        {
            // Arrange
            var instructorNamePartial = "Yoga";
            var allClasses = new List<FitnessClass>
            {
                new FitnessClass { Id = Guid.NewGuid(), InstructorName = "YogaMaster", Title = "Morning Yoga" },
                new FitnessClass { Id = Guid.NewGuid(), InstructorName = "PilatesPro", Title = "Evening Pilates" },
                new FitnessClass { Id = Guid.NewGuid(), InstructorName = "YogaBeginner", Title = "Restorative Yoga" }
            };

            _classRepoMock.Setup(repo => repo.SearchAsync(null, instructorNamePartial)).ReturnsAsync(allClasses.Where(c => c.InstructorName.Contains(instructorNamePartial)).ToList());

            var request = new ClassSearchRequest
            {
                Date = null,
                InstructorName = instructorNamePartial,
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _classService.SearchClassesAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Items.All(c => c.InstructorName.Contains(instructorNamePartial)).Should().BeTrue();
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task SearchClasses_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var allClasses = new List<FitnessClass>();
            for (int i = 1; i <= 10; i++)
            {
                allClasses.Add(new FitnessClass
                {
                    Id = Guid.NewGuid(),
                    ScheduledAt = DateTime.UtcNow.AddDays(i),
                    InstructorName = $"Instructor{i}",
                    Title = $"Class {i}"
                });
            }

            // Repository returns all classes (no date/instructor filter)
            _classRepoMock.Setup(repo => repo.SearchAsync(null, null)).ReturnsAsync(allClasses);

            var request = new ClassSearchRequest
            {
                Date = null,
                InstructorName = null,
                Page = 2,
                PageSize = 3
            };

            // Act
            var result = await _classService.SearchClassesAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Page.Should().Be(2);
            result.PageSize.Should().Be(3);
            result.TotalCount.Should().Be(10);
            result.TotalPages.Should().Be(4); // Ceiling(10/3) = 4
            result.Items.Should().HaveCount(3);
            // Items 4,5,6 (0-index: skip 3, take 3)
            var expectedIds = allClasses.Skip(3).Take(3).Select(c => c.Id).ToList();
            result.Items.Select(i => i.Id).Should().Equal(expectedIds);
        }
    }
}