using System;

namespace TestsDuo2.TestHelpers
{
    public static class UserFactory
    {
        public static DuoClassLibrary.Models.User CreateUser(
            int id = 0, 
            string username = "testuser", 
            string email = "test@example.com",
            string password = "password123",
            bool privacyStatus = false,
            bool onlineStatus = true,
            DateTime? dateJoined = null,
            string profileImage = "profile.jpg",
            int totalPoints = 0,
            int coursesCompleted = 0,
            int quizzesCompleted = 0,
            int streak = 0,
            DateTime? lastActivityDate = null,
            decimal accuracy = 0)
        {
            return new DuoClassLibrary.Models.User
            {
                UserId = id,
                UserName = username,
                Email = email,
                Password = password,
                PrivacyStatus = privacyStatus,
                OnlineStatus = onlineStatus,
                DateJoined = dateJoined ?? DateTime.Now,
                ProfileImage = profileImage,
                TotalPoints = totalPoints,
                CoursesCompleted = coursesCompleted,
                QuizzesCompleted = quizzesCompleted,
                Streak = streak,
                LastActivityDate = lastActivityDate,
                Accuracy = accuracy
            };
        }
    }
} 