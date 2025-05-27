using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DuoClassLibrary.Constants;
using DuoClassLibrary.Models;
using Duo.Services;
using DuolingoNou.Services;
using Moq;
using Xunit;

namespace TestsDuo2.Services
{
    public class CourseServiceTests
    {
        private readonly MockCourseService _courseService;

        public CourseServiceTests()
        {
            _courseService = new MockCourseService();
        }

        [Fact]
        public async Task GetEnrolledCoursesAsync_ReturnsValidCourseList()
        {
            // Arrange
            // No specific arrangement needed as the service is self-contained

            // Act
            var result = await _courseService.GetEnrolledCoursesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<MyCourse>>(result);
            Assert.All(result, course =>
            {
                Assert.NotNull(course.Id);
                Assert.NotNull(course.Name);
                Assert.NotNull(course.Language);
                Assert.InRange(course.CompletionPercentage, 0, 1);
                Assert.True(course.EnrollmentDate <= DateTime.Now);
                Assert.True(course.EnrollmentDate >= DateTime.Now.AddDays(-MockDataConstants.MaximumCompletionDaysInPast));
            });
        }

        [Fact]
        public async Task GetEnrolledCoursesAsync_ReturnsCoursesWithinValidCountRange()
        {
            // Arrange
            // No specific arrangement needed

            // Act
            var result = await _courseService.GetEnrolledCoursesAsync();

            // Assert
            Assert.InRange(result.Count, 
                MockDataConstants.MinimumRandomQuizCount, 
                MockDataConstants.MaximumRandomQuizCount);
        }

        [Fact]
        public async Task GetEnrolledCoursesAsync_CourseIdsAreUnique()
        {
            // Arrange
            // No specific arrangement needed

            // Act
            var result = await _courseService.GetEnrolledCoursesAsync();

            // Assert
            var uniqueIds = new HashSet<string>(result.Select(c => c.Id));
            Assert.Equal(result.Count, uniqueIds.Count);
        }


        [Fact]
        public async Task GetEnrolledCoursesAsync_EnrollmentDatesAreWithinValidRange()
        {
            // Arrange
            var now = DateTime.Now;
            var minDate = now.AddDays(-MockDataConstants.MaximumCompletionDaysInPast);

            // Act
            var result = await _courseService.GetEnrolledCoursesAsync();

            // Assert
            Assert.All(result, course =>
            {
                Assert.True(course.EnrollmentDate <= now);
                Assert.True(course.EnrollmentDate >= minDate);
            });
        }


        [Fact]
        public async Task GetEnrolledCoursesAsync_CompletionPercentagesAreWithinValidRange()
        {
            // Arrange
            // No specific arrangement needed

            // Act
            var result = await _courseService.GetEnrolledCoursesAsync();

            // Assert
            Assert.All(result, course =>
            {
                Assert.InRange(course.CompletionPercentage, 0, 1);
            });
        }
    }
} 