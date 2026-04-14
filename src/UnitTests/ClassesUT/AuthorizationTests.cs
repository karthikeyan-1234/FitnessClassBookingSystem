using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Application.DTOs.Class;
using Application.Interfaces;
using Application.Services;

using Domain.Entities;
using Domain.Interfaces;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Moq;

using Presentation.Controllers;

using Shared;

namespace ClassesUT
{
    public class AuthorizationTests
    {
        private readonly Mock<IClassRepository> _classRepoMock;
        private readonly Mock<IBookingRepository> _bookingRepoMock;
        private readonly Mock<IAccountAPIClient> _accountApiMock;
        private readonly ClassService _classService;

        public AuthorizationTests()
        {
            _classRepoMock = new Mock<IClassRepository>();
            _bookingRepoMock = new Mock<IBookingRepository>();
            _accountApiMock = new Mock<IAccountAPIClient>();
            _classService = new ClassService(_classRepoMock.Object, _bookingRepoMock.Object, _accountApiMock.Object);
        }

        [Fact]
        public async Task CreateClass_WhenInstructorDoesNotExistInAccountApi_ThrowsDomainException()
        {
            // Arrange
            var nonExistentInstructorId = Guid.NewGuid();

            _accountApiMock.Setup(a => a.InstructorExistsAsync(nonExistentInstructorId)).ReturnsAsync(false); // AccountAPI says this instructor doesn't exist

            var request = new CreateClassRequest
            {
                Title = "Yoga Basics",
                Description = "Intro session",
                InstructorId = nonExistentInstructorId,
                ScheduledAt = DateTime.UtcNow.AddDays(1),
                Capacity = 10
            };

            try
            {
                await _classService.CreateClassAsync(request, nonExistentInstructorId!);
            }
            catch (Exception ex)
            {
                //Assert
                ex.Should().BeOfType<DomainException>();
                Assert.Contains("Instructor with ID", ex.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("does not exist", ex.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public async Task UpdateClass_WhenRequestingUserIsNotClassOwner_ThrowsDomainException()
        {
            // Arrange
            var ownerInstructorId = Guid.NewGuid();
            var differentInstructorId = Guid.NewGuid();

            var existingClass = new FitnessClass
            {
                Id = Guid.NewGuid(),
                Title = "Yoga Basics",
                InstructorId = ownerInstructorId, // owned by someone else
                ScheduledAt = DateTime.UtcNow.AddDays(1),
                Capacity = 10,
                BookedCount = 0
            };

            _classRepoMock.Setup(r => r.GetByIdAsync(existingClass.Id)).ReturnsAsync(existingClass);

            var request = new UpdateClassRequest
            {
                Title = "Pilates",
                Description = "Updated",
                ScheduledAt = DateTime.UtcNow.AddDays(2),
                Capacity = 15
            };

            try
            {
                await _classService.UpdateClassAsync(existingClass.Id,request, differentInstructorId);
            }
            catch (Exception ex)
            {
                //Assert
                ex.Should().BeOfType<DomainException>();
                Assert.Contains("You are not authorized to update this class", ex.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public async Task UpdateClass_WhenRequestingUserIsClassOwner_UpdatesClass()
        {
            // Arrange
            var ownerInstructorId = Guid.NewGuid();
            var requestingInstructorId = ownerInstructorId;

            var existingClass = new FitnessClass
            {
                Id = Guid.NewGuid(),
                Title = "Yoga Basics",
                InstructorId = ownerInstructorId, // owned by someone else
                ScheduledAt = DateTime.UtcNow.AddDays(1),
                Capacity = 10,
                BookedCount = 0
            };

            _classRepoMock.Setup(r => r.GetByIdAsync(existingClass.Id)).ReturnsAsync(existingClass);

            var request = new UpdateClassRequest
            {
                Title = "Pilates",
                Description = "Updated",
                ScheduledAt = DateTime.UtcNow.AddDays(2),
                Capacity = 15
            };


            var response = await _classService.UpdateClassAsync(existingClass.Id, request, requestingInstructorId);

            response.Should().NotBeNull();
            response.Should().BeOfType<ClassResponse>();
            response!.Capacity.Should().Be(request.Capacity);

        }

        [Fact]
        public async Task DeleteClass_WhenCalledByDifferentInstructor_ThrowsDomainException()
        {
            // Arrange
            var ownerInstructorId = Guid.NewGuid();
            var differentInstructorId = Guid.NewGuid();

            var existingClass = new FitnessClass
            {
                Id = Guid.NewGuid(),
                Title = "Yoga Basics",
                InstructorId = ownerInstructorId, // owned by someone else
                ScheduledAt = DateTime.UtcNow.AddDays(1),
                Capacity = 10,
                BookedCount = 0
            };

            _classRepoMock.Setup(r => r.GetByIdAsync(existingClass.Id)).ReturnsAsync(existingClass);

            // Act & Assert


            try
            {
                await _classService.DeleteClassAsync(existingClass.Id,differentInstructorId);
            }
            catch (Exception ex)
            {
                //Assert
                ex.Should().BeOfType<DomainException>();
                Assert.Contains("You are not authorized to delete this class", ex.Message, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
