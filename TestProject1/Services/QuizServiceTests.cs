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
    public class QuizServiceTests
    {
        private readonly MockQuizService _quizService;

        public QuizServiceTests()
        {
            _quizService = new MockQuizService();
        }

        [Fact]
        public async Task GetCompletedQuizzesAsync_ReturnsValidQuizList()
        {
            // Arrange
            // No specific arrangement needed as the service is self-contained

            // Act
            var result = await _quizService.GetCompletedQuizzesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<Quiz>>(result);
            Assert.All(result, quiz =>
            {
                Assert.NotNull(quiz.Id);
                Assert.NotNull(quiz.Name);
                Assert.NotNull(quiz.Category);
                Assert.InRange(quiz.AccuracyPercentage, 0, 1);
                Assert.True(quiz.CompletionDate <= DateTime.Now);
                Assert.True(quiz.CompletionDate >= DateTime.Now.AddDays(-MockDataConstants.MaximumCompletionDaysInPast));
            });
        }


        [Fact]
        public async Task GetCompletedQuizzesAsync_ReturnsQuizzesWithinValidCountRange()
        {
            // Arrange
            // No specific arrangement needed

            // Act
            var result = await _quizService.GetCompletedQuizzesAsync();

            // Assert
            Assert.InRange(result.Count, 
                MockDataConstants.MinimumRandomQuizCount, 
                MockDataConstants.MaximumRandomQuizCount);
        }

        [Fact]
        public async Task GetCompletedQuizzesAsync_QuizIdsAreUnique()
        {
            // Arrange
            // No specific arrangement needed

            // Act
            var result = await _quizService.GetCompletedQuizzesAsync();

            // Assert
            var uniqueIds = new HashSet<string>(result.Select(q => q.Id));
            Assert.Equal(result.Count, uniqueIds.Count);
        }

        [Fact]
        public async Task GetCompletedQuizzesAsync_CompletionDatesAreWithinValidRange()
        {
            // Arrange
            var now = DateTime.Now;
            var minDate = now.AddDays(-MockDataConstants.MaximumCompletionDaysInPast);

            // Act
            var result = await _quizService.GetCompletedQuizzesAsync();

            // Assert
            Assert.All(result, quiz =>
            {
                Assert.True(quiz.CompletionDate <= now);
                Assert.True(quiz.CompletionDate >= minDate);
            });
        }
    }
} 