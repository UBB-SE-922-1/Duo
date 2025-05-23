using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using DuolingoNou.Views.Pages;
using Duo.ViewModels;
using Duo.Services.Interfaces;
using Duo.Views.Pages;
using Duo.Services;
using Duo;
using DuolingoClassLibrary.Services.Interfaces;

namespace DuolingoNou.Views
{
    public sealed partial class ShellPage : Page
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;

        public ShellPage()
        {
            this.InitializeComponent();
            
            // Initialize services
            _userService = App.userService;
            _postService = App._postService;
            _categoryService = App._categoryService;

            // Set up event handlers
            NavView.SelectionChanged += NavView_SelectionChanged;
            SearchBox.TextChanged += SearchBox_TextChanged;

            // Set initial navigation
            ContentFrame.Navigate(typeof(ProfileSettingsPage));
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                switch (selectedItem.Tag)
                {
                    case "Settings":
                        ContentFrame.Navigate(typeof(ProfileSettingsPage));
                        break;
                    case "HomePage":
                        ContentFrame.Navigate(typeof(MainPage));
                        break;
                    case "Leaderboards":
                        ContentFrame.Navigate(typeof(LeaderboardPage));
                        break;
                    case "Stats":
                        ContentFrame.Navigate(typeof(AchievementsPage));
                        break;
                    case "Course":
                        ContentFrame.Navigate(typeof(CoursePage));
                        break;
                    case "Quiz":
                        ContentFrame.Navigate(typeof(QuizPage));
                        break;
                    case "Announcements":
                        ContentFrame.Navigate(typeof(PostListPage), "Announcements");
                        break;
                    case "Discover":
                        ContentFrame.Navigate(typeof(PostListPage), "Discover");
                        break;
                    case "GeneralDiscussion":
                        ContentFrame.Navigate(typeof(PostListPage), "General-Discussion");
                        break;
                    case "LessonHelp":
                        ContentFrame.Navigate(typeof(PostListPage), "Lesson-Help");
                        break;
                    case "OffTopic":
                        ContentFrame.Navigate(typeof(PostListPage), "Off-topic");
                        break;
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Implement search functionality
            string searchText = SearchBox.Text;
            // Add search logic here
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchBox.Text?.Trim();

            if (!string.IsNullOrEmpty(query))
            {
                // TODO: Implement search functionality
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Search Triggered",
                    Content = $"You searched for: {query}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                _ = dialog.ShowAsync();
            }
            else
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Empty Search",
                    Content = "Please enter a search term.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                _ = dialog.ShowAsync();
            }
        }
    }
}