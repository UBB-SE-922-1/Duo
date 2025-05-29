// <copyright file="LeaderboardPage.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Duo.ViewModels;
    using DuoClassLibrary.Models;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A page that displays the leaderboard.
    /// </summary>
    public sealed partial class LeaderboardPage : Page
    {

        private readonly LeaderboardViewModel leaderboardViewModel;
        private readonly int currentUserId = App.CurrentUser.UserId;
        private string selectedMode = "Global";

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaderboardPage"/> class.
        /// </summary>
        public LeaderboardPage()
        {
            this.InitializeComponent();
            this.leaderboardViewModel = App.ServiceProvider.GetRequiredService<LeaderboardViewModel>();
            this.Leaderboard = new ObservableCollection<LeaderboardEntry>();
            this.LeaderboardListView.ItemsSource = this.Leaderboard;
            this.CurrentUserRank.Text = $"Your Rank: {this.leaderboardViewModel.GetCurrentUserGlobalRank(this.currentUserId, "Accuracy")}";
        }

        /// <summary>
        /// Gets or sets the leaderboard entries.
        /// </summary>
        public ObservableCollection<LeaderboardEntry> Leaderboard { get; set; }

        private async void GlobalButton_Click(object sender, RoutedEventArgs e)
        {
            this.selectedMode = "Global";
            this.LeaderboardListView.ItemsSource = await this.leaderboardViewModel.GetGlobalLeaderboard("Accuracy");
            this.CurrentUserRank.Text = $"Your Rank: {await this.leaderboardViewModel.GetCurrentUserGlobalRank(this.currentUserId, "Accuracy")}";
            this.RankingCriteriaComboBox.SelectedItem = this.SortBy;
        }

        private async void FriendsButton_Click(object sender, RoutedEventArgs e)
        {
            this.selectedMode = "Friends";
            var friendsLeaderboard = await this.leaderboardViewModel.GetFriendsLeaderboard(this.currentUserId, "Accuracy");
            this.LeaderboardListView.ItemsSource = friendsLeaderboard;
            this.CurrentUserRank.Text = $"Your Rank: {await this.leaderboardViewModel.GetCurrentUserFriendsRank(this.currentUserId, "Accuracy")}";
            this.RankingCriteriaComboBox.SelectedItem = this.SortBy;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedMode == "Global")
            {
                this.Leaderboard = new ObservableCollection<LeaderboardEntry>(await this.leaderboardViewModel.GetGlobalLeaderboard("Accuracy"));
                this.CurrentUserRank.Text = $"Your Rank: {this.leaderboardViewModel.GetCurrentUserGlobalRank(this.currentUserId, "Accuracy")}";
            }
            else
            {
                var friendsLeaderboard = await this.leaderboardViewModel.GetFriendsLeaderboard(this.currentUserId, "Accuracy");
                this.Leaderboard = new ObservableCollection<LeaderboardEntry>(friendsLeaderboard);
                this.CurrentUserRank.Text = $"Your Rank: {await this.leaderboardViewModel.GetCurrentUserFriendsRank(this.currentUserId, "Accuracy")}";
            }
            this.LeaderboardListView.ItemsSource = this.Leaderboard;
            this.RankingCriteriaComboBox.SelectedItem = this.SortBy;
        }

        private async void RankingCriteriaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.RankingCriteriaComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedCriteria = selectedItem.Content.ToString();
                switch (selectedCriteria)
                {
                    case "Accuracy":
                        if (this.selectedMode == "Global")
                        {
                            this.Leaderboard = new ObservableCollection<LeaderboardEntry>(await this.leaderboardViewModel.GetGlobalLeaderboard("Accuracy"));
                            this.CurrentUserRank.Text = $"Your Rank: {this.leaderboardViewModel.GetCurrentUserGlobalRank(this.currentUserId, "Accuracy")}";
                        }
                        else
                        {
                            var friendsLeaderboard = await this.leaderboardViewModel.GetFriendsLeaderboard(this.currentUserId, "Accuracy");
                            this.Leaderboard = new ObservableCollection<LeaderboardEntry>(friendsLeaderboard);
                            this.CurrentUserRank.Text = $"Your Rank: {await this.leaderboardViewModel.GetCurrentUserFriendsRank(this.currentUserId, "Accuracy")}";
                        }
                        this.LeaderboardListView.ItemsSource = this.Leaderboard;
                        break;

                    case "CompletedQuizzes":
                        if (this.selectedMode == "Global")
                        {
                            this.Leaderboard = new ObservableCollection<LeaderboardEntry>(await this.leaderboardViewModel.GetGlobalLeaderboard("CompletedQuizzes"));
                            this.CurrentUserRank.Text = $"Your Rank: {this.leaderboardViewModel.GetCurrentUserGlobalRank(this.currentUserId, "CompletedQuizzes")}";
                        }
                        else
                        {
                            var friendsLeaderboard = await this.leaderboardViewModel.GetFriendsLeaderboard(this.currentUserId, "CompletedQuizzes");
                            this.Leaderboard = new ObservableCollection<LeaderboardEntry>(friendsLeaderboard);
                            this.CurrentUserRank.Text = $"Your Rank: {await this.leaderboardViewModel.GetCurrentUserFriendsRank(this.currentUserId, "CompletedQuizzes")}";
                        }
                        this.LeaderboardListView.ItemsSource = this.Leaderboard;
                        break;
                }
            }
        }
    }
}