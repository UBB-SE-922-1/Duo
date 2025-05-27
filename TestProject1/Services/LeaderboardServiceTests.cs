using System;
using System.Collections.Generic;
using DuoClassLibrary.Constants;
using DuoClassLibrary.Models;
using Duo.Services;
using Moq;
using Xunit;
using DuoClassLibrary.Services.Interfaces;

namespace TestProject1.Services
{
    public class LeaderboardServiceTests
    {
        private readonly Mock<IUserHelperService> _userHelperServiceMock;
        private readonly Mock<IFriendsService> _friendsServiceMock;
        private readonly LeaderboardService _leaderboardService;

        public LeaderboardServiceTests()
        {
            _userHelperServiceMock = new Mock<IUserHelperService>();
            _friendsServiceMock = new Mock<IFriendsService>();
            _leaderboardService = new LeaderboardService(_userHelperServiceMock.Object, _friendsServiceMock.Object);
        }

        [Fact]
        public async Task GetGlobalLeaderboard_CompletedQuizzesCriteria_CallsCorrectMethod()
        {
            // Arrange
            var expectedLeaderboard = new List<LeaderboardEntry>();
            _userHelperServiceMock.Setup(x => x.GetTopUsersByCompletedQuizzes())
                .ReturnsAsync(expectedLeaderboard);

            // Act
            await _leaderboardService.GetGlobalLeaderboard(LeaderboardConstants.CompletedQuizzesCriteria);

            // Assert
            _userHelperServiceMock.Verify(x => x.GetTopUsersByCompletedQuizzes(), Times.Once);
        }

        [Fact]
        public async Task GetGlobalLeaderboard_AccuracyCriteria_CallsCorrectMethod()
        {
            // Arrange
            var expectedLeaderboard = new List<LeaderboardEntry>();
            _userHelperServiceMock.Setup(x => x.GetTopUsersByAccuracy())
                .ReturnsAsync(expectedLeaderboard);

            // Act
            await _leaderboardService.GetGlobalLeaderboard(LeaderboardConstants.AccuracyCriteria);

            // Assert
            _userHelperServiceMock.Verify(x => x.GetTopUsersByAccuracy(), Times.Once);
        }

        [Fact]
        public async Task GetGlobalLeaderboard_InvalidCriteria_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _leaderboardService.GetGlobalLeaderboard("InvalidCriteria"));
        }

        [Fact]
        public async Task GetFriendsLeaderboard_CompletedQuizzesCriteria_CallsCorrectMethod()
        {
            // Arrange
            const int userId = 1;
            var expectedLeaderboard = new List<LeaderboardEntry>();
            _friendsServiceMock.Setup(x => x.GetTopFriendsByCompletedQuizzes(userId))
                .ReturnsAsync(expectedLeaderboard);

            // Act
            await _leaderboardService.GetFriendsLeaderboard(userId, LeaderboardConstants.CompletedQuizzesCriteria);

            // Assert
            _friendsServiceMock.Verify(x => x.GetTopFriendsByCompletedQuizzes(userId), Times.Once);
        }

        [Fact]
        public async Task GetFriendsLeaderboard_AccuracyCriteria_CallsCorrectMethod()
        {
            // Arrange
            const int userId = 1;
            var expectedLeaderboard = new List<LeaderboardEntry>();
            _friendsServiceMock.Setup(x => x.GetTopFriendsByAccuracy(userId))
                .ReturnsAsync(expectedLeaderboard);

            // Act
            await _leaderboardService.GetFriendsLeaderboard(userId, LeaderboardConstants.AccuracyCriteria);

            // Assert
            _friendsServiceMock.Verify(x => x.GetTopFriendsByAccuracy(userId), Times.Once);
        }

        [Fact]
        public async Task GetFriendsLeaderboard_InvalidCriteria_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _leaderboardService.GetFriendsLeaderboard(1, "InvalidCriteria"));
        }

        [Fact]
        public async Task UpdateUserScore_UserExists_UpdatesUser()
        {
            // Arrange
            const int userId = 1;
            const int points = 10;
            var user = new User { UserId = userId };
            
            _userHelperServiceMock.Setup(x => x.GetUserById(userId))
                .ReturnsAsync(user);

            // Act
            await _leaderboardService.UpdateUserScore(userId, points);

            // Assert
            _userHelperServiceMock.Verify(x => x.UpdateUser(user), Times.Once);
        }

        [Fact]
        public async Task UpdateUserScore_UserDoesNotExist_DoesNotUpdateUser()
        {
            // Arrange
            const int userId = 1;
            const int points = 10;
            
            _userHelperServiceMock.Setup(x => x.GetUserById(userId))
                .ReturnsAsync((User)null);

            // Act
            await _leaderboardService.UpdateUserScore(userId, points);

            // Assert
            _userHelperServiceMock.Verify(x => x.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task CalculateRankChange_Called_CompletesSuccessfully()
        {
            // Act
            await _leaderboardService.CalculateRankChange(1, "weekly");

            // Assert
            Assert.True(true); // Just verifying it completes without throwing
        }
    }
} 