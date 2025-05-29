// <copyright file="CategoryPage.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Duo.ViewModels;
    using Duo.Views.Components;
    using DuoClassLibrary.Models;
    using DuolingoNou.Views.Pages;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    /// <summary>
    /// Displays categories and handles navigation for the main community page.
    /// </summary>
    public sealed partial class CategoryPage : Page
    {
        private CategoryPageViewModel viewModel = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryPage"/> class.
        /// </summary>
        public CategoryPage()
        {
            try
            {
                this.InitializeComponent();
                this.NavigationCacheMode = NavigationCacheMode.Required;
                _ = this.InitializeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Page initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Refreshes the current view, navigating to the current category or main page.
        /// </summary>
        public void RefreshCurrentView()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.viewModel.CurrentCategoryName))
                {
                    this.contentFrame.Navigate(typeof(PostListPage), this.viewModel.CurrentCategoryName);
                }
                else
                {
                    this.contentFrame.Navigate(typeof(MainPage));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Refresh current view failed: {ex.Message}");
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                this.viewModel = new CategoryPageViewModel();
                this.DataContext = this.viewModel;

                // Subscribe to events
                this.viewModel.NavigationRequested += this.OnNavigationRequested;
                this.viewModel.CategoryNavigationRequested += this.OnCategoryNavigationRequested;
                this.viewModel.PostCreationSucceeded += this.OnPostCreationSucceeded;

                // Wait for categories to load
                while (this.viewModel.IsLoading)
                {
                    await Task.Delay(100);
                }

                this.PopulateCommunityMenuItems();

                // Set the username from the view model
                this.UsernameTextBlock.Text = this.viewModel.Username;

                try
                {
                    this.contentFrame.Navigate(typeof(MainPage));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Initial navigation failed: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Async initialization failed: {ex.Message}");
            }
        }

        private void PopulateCommunityMenuItems()
        {
            try
            {
                var categoryNames = this.viewModel.CategoryNames;
                if (categoryNames == null || categoryNames.Count == 0)
                {
                    Debug.WriteLine("No categories found or categoryNames is null");
                    return;
                }

                CommunityItem.MenuItems.Clear();

                foreach (string categoryName in categoryNames)
                {
                    if (string.IsNullOrEmpty(categoryName))
                    {
                        Debug.WriteLine("Skipping null or empty category name");
                        continue;
                    }

                    var item = new NavigationViewItem
                    {
                        Content = categoryName.Replace("-", " "),
                        Icon = new SymbolIcon(Symbol.Message),
                        Tag = categoryName,
                    };

                    ToolTipService.SetToolTip(item, categoryName.Replace("-", " "));
                    CommunityItem.MenuItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error populating community menu items: {ex.Message}");
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem selectedItem)
            {
                switch (selectedItem.Tag)
                {
                    case "Profile":
                        this.contentFrame.Navigate(typeof(ProfileSettingsPage));
                        break;
                    case "Settings":
                        this.contentFrame.Navigate(typeof(ProfileSettingsPage));
                        break;
                    case "HomePage":
                        this.contentFrame.Navigate(typeof(MainPage));
                        break;
                    case "QuizParent":
                        this.contentFrame.Navigate(typeof(Views.Pages.RoadmapMainPage), "Roadmap");
                        break;
                    case "QuizAdminParent":
                        this.contentFrame.Navigate(typeof(Views.Pages.AdminMainPage), "Admin");
                        break;
                    case "CoursesParent":
                        this.contentFrame.Navigate(typeof(Duo.Views.MainPage), "Main");
                        break;
                    default:
                        if (selectedItem.Tag is string categoryName)
                        {
                            this.contentFrame.Navigate(typeof(PostListPage), categoryName);
                        }

                        break;
                }
            }
        }

        private void OnNavigationRequested(object? sender, Type pageType)
        {
            try
            {
                this.contentFrame.Navigate(pageType);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation failed: {ex.Message}");
            }
        }

        private void OnCategoryNavigationRequested(object? sender, string category)
        {
            try
            {
                this.contentFrame.Navigate(typeof(PostListPage), category);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Category navigation failed: {ex.Message}");
            }
        }

        private void OnPostCreationSucceeded(object? sender, bool success)
        {
            // This would handle any UI updates needed when post creation succeeds
            // Currently nothing specific is needed here as we navigate to refresh the list
        }

        private async void CreatePostBtn_CreatePostRequested(object? sender, RoutedEventArgs e)
        {
            var dialogComponent = new DialogComponent();
            var result = await dialogComponent.ShowCreatePostDialog(this.XamlRoot, this.viewModel.CurrentCategoryId);

            if (result.Success)
            {
                var successDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Success",
                    Content = "Your post was created successfully!",
                    CloseButtonText = "OK",
                };
                await successDialog.ShowAsync();

                this.viewModel.HandlePostCreation(true);
            }
        }
    }
}