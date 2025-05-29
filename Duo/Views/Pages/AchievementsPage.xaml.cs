// <copyright file="AchievementsPage.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace DuolingoNou.Views.Pages
{
    using System;
    using System.Threading.Tasks;
    using Duo;
    using Duo.ViewModels;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// Page displaying user achievements and statistics.
    /// </summary>
    public sealed partial class AchievementsPage : Page
    {
        private readonly ProfileViewModel viewModel;
        private readonly ProfileService profileService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementsPage"/> class.
        /// </summary>
        public AchievementsPage()
        {
            this.InitializeComponent();
            this.viewModel = App.ServiceProvider.GetRequiredService<ProfileViewModel>();
            this.profileService = App.ServiceProvider.GetRequiredService<ProfileService>();

            this.LoadUserStats();
            _ = this.LoadUserAchievements();
        }

        /// <summary>
        /// Loads and displays user statistics.
        /// </summary>
        private void LoadUserStats()
        {
            this.TotalXPText.Text = "Total XP: ";
            this.BestStreakText.Text = "Best Streak: ";
            this.QuizzesCompletedText.Text = "Quizzes Completed: ";
            this.CoursesCompletedText.Text = "Courses Completed: ";
        }

        /// <summary>
        /// Loads user achievements asynchronously.
        /// </summary>
        private async Task LoadUserAchievements()
        {
            User currentUser = App.CurrentUser;

            // Implementation for loading achievements goes here.
        }
    }
}