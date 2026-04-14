using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Application.Interfaces;
using Application.Services;

using Domain.Entities;
using Domain.Interfaces;

using FluentAssertions;

using Moq;

using Shared;


namespace ClassesUT
{
    public class PaginationTests
    {
        private readonly Mock<IClassRepository> _classRepoMock;
        private readonly Mock<IBookingRepository> _bookingRepoMock;
        private readonly Mock<IAccountAPIClient> _accountApiMock;
        private readonly ClassService _classService;

        public PaginationTests()
        {
            _classRepoMock = new Mock<IClassRepository>();
            _bookingRepoMock = new Mock<IBookingRepository>();
            _accountApiMock = new Mock<IAccountAPIClient>();
            _classService = new ClassService(_classRepoMock.Object, _bookingRepoMock.Object, _accountApiMock.Object);
        }

        [Fact]
        public async Task GetClassesForMember_WhenMultipleBookings_ReturnsCorrectPage()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var classCount = 10;
            var page = 2;
            var pageSize = 3;

            // Create 10 unique classes
            var classes = new List<FitnessClass>();
            for (int i = 1; i <= classCount; i++)
            {
                var classId = Guid.NewGuid();
                classes.Add(new FitnessClass
                {
                    Id = classId,
                    Title = $"Class {i}",
                    InstructorId = Guid.NewGuid(),
                    InstructorName = $"Instructor {i}",
                    ScheduledAt = DateTime.UtcNow.AddDays(i),
                    Capacity = 10,
                    BookedCount = 0
                });
            }

            // Create bookings for all classes (member booked each class)
            var bookings = classes.Select(c => new Booking
            {
                Id = Guid.NewGuid(),
                ClassId = c.Id,
                MemberId = memberId,
                BookedAt = DateTime.UtcNow
            }).ToList();

            _bookingRepoMock.Setup(repo => repo.GetByMemberAsync(memberId)).ReturnsAsync(bookings);

            // Setup GetByIdAsync to return the correct class when called
            foreach (var c in classes)
            {
                _classRepoMock.Setup(repo => repo.GetByIdAsync(c.Id)).ReturnsAsync(c);
            }

            // Act
            var result = await _classService.GetClassesForMemberAsync(memberId, page, pageSize);

            // Assert
            result.Should().NotBeNull();
            result.Page.Should().Be(page);
            result.PageSize.Should().Be(pageSize);
            result.TotalCount.Should().Be(classCount);
            result.TotalPages.Should().Be((int)Math.Ceiling(classCount / (double)pageSize));

            // Expect items 4,5,6 (0-index: skip = (2-1)*3 = 3, take 3)
            var expectedIds = classes.Skip(3).Take(3).Select(c => c.Id).ToList(); //Manually find the list by skipping and taking the relevant data
            result.Items.Select(i => i.Id).Should().Equal(expectedIds); // Now use the data to check against the data returned by service
        }


        [Fact]
        public async Task GetClassesForMember_WhenPageSizeExceedsTotal_ReturnsAllItems()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var classCount = 5;
            var requestedPage = 1;
            var requestedPageSize = 20; // larger than total, total entries are only 5 but we are asking for 50 entries in 1 page

            // Create only 5 classes
            var classes = new List<FitnessClass>();
            for (int i = 1; i <= classCount; i++)
            {
                var classId = Guid.NewGuid();
                classes.Add(new FitnessClass
                {
                    Id = classId,
                    Title = $"Class {i}",
                    InstructorId = Guid.NewGuid(),
                    InstructorName = $"Instructor {i}",
                    ScheduledAt = DateTime.UtcNow.AddDays(i),
                    Capacity = 10,
                    BookedCount = 0
                });
            }

            var bookings = classes.Select(c => new Booking
                            {
                                Id = Guid.NewGuid(),
                                ClassId = c.Id,
                                MemberId = memberId,
                                BookedAt = DateTime.UtcNow
                            }).ToList();

            _bookingRepoMock.Setup(repo => repo.GetByMemberAsync(memberId)).ReturnsAsync(bookings);

            foreach (var c in classes)
            {
                _classRepoMock.Setup(repo => repo.GetByIdAsync(c.Id)).ReturnsAsync(c);
            }

            // Act
            var result = await _classService.GetClassesForMemberAsync(memberId, requestedPage, requestedPageSize);

            // Assert
            result.Should().NotBeNull();
            result.Page.Should().Be(requestedPage);
            result.PageSize.Should().Be(requestedPageSize);
            result.TotalCount.Should().Be(classCount);
            result.TotalPages.Should().Be(1); // because pageSize > totalCount
            result.Items.Count.Should().Be(classCount); // all items returned
            result.Items.Select(i => i.Id).Should().Equal(classes.Select(c => c.Id));
        }

    }
}
