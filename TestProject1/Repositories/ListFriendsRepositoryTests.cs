using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duo.Api.Persistence;
using DuoClassLibrary.Models;
using Duo.Api.Repositories.Repos;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TestProject1.Repositories
{
    public class ListFriendsRepositoryTests : IDisposable
    {
        private readonly DataContext _mockContext;
        private readonly FriendsRepository _repository;

        public ListFriendsRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockContext = new DataContext(options);
            _repository = new FriendsRepository(_mockContext);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Clear existing data
            _mockContext.Friends.RemoveRange(_mockContext.Friends);
            _mockContext.Users.RemoveRange(_mockContext.Users);
            _mockContext.SaveChanges();

            // Add test users
            var users = new List<User>
            {
                new User 
                { 
                    UserId = 1, 
                    UserName = "friend1", 
                    DateJoined = new DateTime(2023, 1, 15),
                    OnlineStatus = true,
                    LastActivityDate = DateTime.Now.AddMinutes(-5),
                    QuizzesCompleted = 10,
                    Accuracy = 95.5m
                },
                new User 
                { 
                    UserId = 2, 
                    UserName = "friend2", 
                    DateJoined = new DateTime(2023, 2, 20),
                    OnlineStatus = false,
                    LastActivityDate = DateTime.Now.AddHours(-2),
                    QuizzesCompleted = 8,
                    Accuracy = 92.0m
                },
                new User 
                { 
                    UserId = 3, 
                    UserName = "friend3", 
                    DateJoined = new DateTime(2022, 12, 10),
                    OnlineStatus = false,
                    LastActivityDate = DateTime.Now.AddDays(-1),
                    QuizzesCompleted = 5,
                    Accuracy = 88.5m
                }
            };

            _mockContext.Users.AddRange(users);
            _mockContext.SaveChanges();

            // Add test friendships
            var friendships = new List<Friend>
            {
                new Friend { FriendshipId = 1, UserId1 = 1, UserId2 = 2 },
                new Friend { FriendshipId = 2, UserId1 = 1, UserId2 = 3 },
                new Friend { FriendshipId = 3, UserId1 = 2, UserId2 = 3 }
            };

            _mockContext.Friends.AddRange(friendships);
            _mockContext.SaveChanges();
        }

        public void Dispose()
        {
            _mockContext.Dispose();
        }


        [Fact]
        public async Task GetFriends_WithValidUserId_ReturnsFriends()
        {
            // Act
            var friends = await _repository.GetFriends(1);

            // Assert
            Assert.NotNull(friends);
            Assert.Equal(2, friends.Count());
        }

        [Fact]
        public async Task GetFriends_WithInvalidUserId_ReturnsEmptyList()
        {
            // Act
            var friends = await _repository.GetFriends(999);

            // Assert
            Assert.NotNull(friends);
            Assert.Empty(friends);
        }

        [Fact]
        public async Task AddFriend_WithValidIds_ReturnsTrue()
        {
            // Act
            var result = await _repository.AddFriend(1, 4);

            // Assert
            Assert.True(result);
            var friendship = await _mockContext.Friends.FirstOrDefaultAsync(f => 
                (f.UserId1 == 1 && f.UserId2 == 4) || 
                (f.UserId1 == 4 && f.UserId2 == 1));
            Assert.NotNull(friendship);
        }

        [Fact]
        public async Task AddFriend_WithSameIds_ReturnsFalse()
        {
            // Act
            var result = await _repository.AddFriend(1, 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddFriend_WithExistingFriendship_ReturnsFalse()
        {
            // Act
            var result = await _repository.AddFriend(1, 2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveFriend_WithValidIds_ReturnsTrue()
        {
            // Act
            var result = await _repository.RemoveFriend(1, 2);

            // Assert
            Assert.True(result);
            var friendship = await _mockContext.Friends.FirstOrDefaultAsync(f => 
                (f.UserId1 == 1 && f.UserId2 == 2) || 
                (f.UserId1 == 2 && f.UserId2 == 1));
            Assert.Null(friendship);
        }

        [Fact]
        public async Task RemoveFriend_WithNonExistentFriendship_ReturnsFalse()
        {
            // Act
            var result = await _repository.RemoveFriend(1, 999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsFriend_WithExistingFriendship_ReturnsTrue()
        {
            // Act
            var result = await _repository.IsFriend(1, 2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsFriend_WithNonExistentFriendship_ReturnsFalse()
        {
            // Act
            var result = await _repository.IsFriend(1, 999);

            // Assert
            Assert.False(result);
        }
    }
} 