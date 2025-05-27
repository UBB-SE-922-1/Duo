using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DuoClassLibrary.Models;
using Duo.Services;
using DuoClassLibrary.Repositories.Interfaces;
using Duo.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace TestsDuo2.Services
{
    public class FriendsServiceTests
    {
        private readonly Mock<IFriendsRepository> _mockFriendsRepository;
        private readonly FriendsService _friendsService;
        private readonly DbContextOptions<DataContext> _options;

        public FriendsServiceTests()
        {
            _mockFriendsRepository = new Mock<IFriendsRepository>();
            _options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _friendsService = new FriendsService(_mockFriendsRepository.Object, new DataContext(_options));
        }

        [Fact]
        public async Task GetFriends_WhenUserHasFriends_ReturnsFriendsList()
        {
            // Arrange
            var userId = 1;
            var friends = new List<Friend>
            {
                new Friend { UserId1 = 1, UserId2 = 2 },
                new Friend { UserId1 = 1, UserId2 = 3 }
            };
            var users = new List<User>
            {
                new User { UserId = 2, UserName = "Friend1" },
                new User { UserId = 3, UserName = "Friend2" }
            };

            _mockFriendsRepository.Setup(x => x.GetFriends(userId))
                .ReturnsAsync(friends);

            using (var context = new DataContext(_options))
            {
                context.Users.AddRange(users);
                await context.SaveChangesAsync();

                // Act
                var result = await _friendsService.GetFriends(userId);

                // Assert
                Assert.Equal(2, result.Count);
            }
        }

        [Fact]
        public async Task GetFriends_WhenUserHasNoFriends_ReturnsEmptyList()
        {
            // Arrange
            var userId = 1;
            var friends = new List<Friend>();

            _mockFriendsRepository.Setup(x => x.GetFriends(userId))
                .ReturnsAsync(friends);

            using (var context = new DataContext(_options))
            {
                // Act
                var result = await _friendsService.GetFriends(userId);

                // Assert
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task SortFriendsByName_ReturnsSortedList()
        {
            // Arrange
            var userId = 1;
            var friends = new List<Friend>
            {
                new Friend { UserId1 = 1, UserId2 = 2 },
                new Friend { UserId1 = 1, UserId2 = 3 }
            };
            var users = new List<User>
            {
                new User { UserId = 2, UserName = "Zebra" },
                new User { UserId = 3, UserName = "Apple" }
            };

            _mockFriendsRepository.Setup(x => x.GetFriends(userId))
                .ReturnsAsync(friends);

            using (var context = new DataContext(_options))
            {
                context.Users.AddRange(users);
                await context.SaveChangesAsync();

                // Act
                var result = await _friendsService.SortFriendsByName(userId);

                // Assert
                Assert.Equal("Apple", result[0].UserName);
            }
        }

        [Fact]
        public async Task SortFriendsByDateAdded_ReturnsSortedList()
        {
            // Arrange
            var userId = 1;
            var friends = new List<Friend>
            {
                new Friend { UserId1 = 1, UserId2 = 2 },
                new Friend { UserId1 = 1, UserId2 = 3 }
            };
            var users = new List<User>
            {
                new User { UserId = 2, UserName = "Friend1", DateJoined = DateTime.Now.AddDays(-2) },
                new User { UserId = 3, UserName = "Friend2", DateJoined = DateTime.Now.AddDays(-1) }
            };

            _mockFriendsRepository.Setup(x => x.GetFriends(userId))
                .ReturnsAsync(friends);

            using (var context = new DataContext(_options))
            {
                context.Users.AddRange(users);
                await context.SaveChangesAsync();

                // Act
                var result = await _friendsService.SortFriendsByDateAdded(userId);

                // Assert
                Assert.Equal("Friend1", result[0].UserName);
            }
        }

        [Fact]
        public async Task SortFriendsByOnlineStatus_ReturnsSortedList()
        {
            // Arrange
            var userId = 1;
            var friends = new List<Friend>
            {
                new Friend { UserId1 = 1, UserId2 = 2 },
                new Friend { UserId1 = 1, UserId2 = 3 }
            };
            var users = new List<User>
            {
                new User { UserId = 2, UserName = "Friend1", OnlineStatus = false },
                new User { UserId = 3, UserName = "Friend2", OnlineStatus = true }
            };

            _mockFriendsRepository.Setup(x => x.GetFriends(userId))
                .ReturnsAsync(friends);

            using (var context = new DataContext(_options))
            {
                context.Users.AddRange(users);
                await context.SaveChangesAsync();

                // Act
                var result = await _friendsService.SortFriendsByOnlineStatus(userId);

                // Assert
                Assert.Equal("Friend2", result[0].UserName);
            }
        }

        [Fact]
        public async Task GetTopFriendsByCompletedQuizzes_ReturnsSortedList()
        {
            // Arrange
            var userId = 1;
            var friends = new List<Friend>
            {
                new Friend { UserId1 = 1, UserId2 = 2 },
                new Friend { UserId1 = 1, UserId2 = 3 }
            };
            var users = new List<User>
            {
                new User { UserId = 2, UserName = "Friend1", QuizzesCompleted = 5 },
                new User { UserId = 3, UserName = "Friend2", QuizzesCompleted = 10 }
            };

            _mockFriendsRepository.Setup(x => x.GetFriends(userId))
                .ReturnsAsync(friends);

            using (var context = new DataContext(_options))
            {
                context.Users.AddRange(users);
                await context.SaveChangesAsync();

                // Act
                var result = await _friendsService.GetTopFriendsByCompletedQuizzes(userId);

                // Assert
                Assert.Equal("Friend2", result[0].Username);
            }
        }

        [Fact]
        public async Task GetTopFriendsByAccuracy_ReturnsSortedList()
        {
            // Arrange
            var userId = 1;
            var friends = new List<Friend>
            {
                new Friend { UserId1 = 1, UserId2 = 2 },
                new Friend { UserId1 = 1, UserId2 = 3 }
            };
            var users = new List<User>
            {
                new User { UserId = 2, UserName = "Friend1", Accuracy = 0.5m },
                new User { UserId = 3, UserName = "Friend2", Accuracy = 0.8m }
            };

            _mockFriendsRepository.Setup(x => x.GetFriends(userId))
                .ReturnsAsync(friends);

            using (var context = new DataContext(_options))
            {
                context.Users.AddRange(users);
                await context.SaveChangesAsync();

                // Act
                var result = await _friendsService.GetTopFriendsByAccuracy(userId);

                // Assert
                Assert.Equal("Friend2", result[0].Username);
            }
        }

        [Fact]
        public async Task GetAllFriendsWithDetails_ReturnsCompleteList()
        {
            // Arrange
            var userId = 1;
            var friends = new List<Friend>
            {
                new Friend { UserId1 = 1, UserId2 = 2 },
                new Friend { UserId1 = 1, UserId2 = 3 }
            };
            var users = new List<User>
            {
                new User { UserId = 2, UserName = "Friend1" },
                new User { UserId = 3, UserName = "Friend2" }
            };

            _mockFriendsRepository.Setup(x => x.GetFriends(userId))
                .ReturnsAsync(friends);

            using (var context = new DataContext(_options))
            {
                context.Users.AddRange(users);
                await context.SaveChangesAsync();

                // Act
                var result = await _friendsService.GetAllFriendsWithDetails(userId);

                // Assert
                Assert.Equal(2, result.Count);
            }
        }
    }
} 