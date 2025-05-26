// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryPageViewModel.cs" company="YourCompanyName">
//   Copyright (c) YourCompanyName. All rights reserved.
//   Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   ViewModel for managing category-related functionality in the application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.ViewModels.Base;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Services.Interfaces;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// ViewModel for managing category-related functionality in the application.
    /// </summary>
    public class CategoryPageViewModel : ViewModelBase
    {
        private readonly ICategoryService categoryService;
        private readonly IUserService userService;
        private readonly CategoryViewModel categoryViewModel;
        private int currentCategoryId = 0;
        private string currentCategoryName = string.Empty;
        private string username = "Guest";
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryPageViewModel"/> class.
        /// </summary>
        public CategoryPageViewModel()
        {
            this.categoryService = App._categoryService;
            this.userService = App.userService;
            this.categoryViewModel = new CategoryViewModel(this.categoryService);
            _ = this.InitializeAsync();
            this.SelectCategoryCommand = new RelayCommandWithParameter<string>(this.SelectCategory);
            this.CreatePostCommand = new RelayCommand(_ => this.CreatePost());
        }

        /// <summary>
        /// Occurs when navigation to a specific page is requested.
        /// </summary>
        public event EventHandler<Type> NavigationRequested = (sender, e) => { };

        /// <summary>
        /// Occurs when navigation to a specific category is requested.
        /// </summary>
        public event EventHandler<string> CategoryNavigationRequested = (sender, e) => { };

        /// <summary>
        /// Occurs when a post creation operation succeeds.
        /// </summary>
        public event EventHandler<bool> PostCreationSucceeded = (sender, e) => { };

        /// <summary>
        /// Gets or sets a value indicating whether the ViewModel is in a loading state.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets the username of the current user.
        /// </summary>
        public string Username
        {
            get => this.username;
            set => this.SetProperty(ref this.username, value);
        }

        /// <summary>
        /// Gets or sets the ID of the currently selected category.
        /// </summary>
        public int CurrentCategoryId
        {
            get => this.currentCategoryId;
            set => this.SetProperty(ref this.currentCategoryId, value);
        }

        /// <summary>
        /// Gets or sets the name of the currently selected category.
        /// </summary>
        public string CurrentCategoryName
        {
            get => this.currentCategoryName;
            set => this.SetProperty(ref this.currentCategoryName, value);
        }

        /// <summary>
        /// Gets the list of category names available for selection.
        /// </summary>
        public List<string> CategoryNames => this.categoryViewModel.GetCategoryNames();

        /// <summary>
        /// Gets command for selecting a category.
        /// </summary>
        public ICommand SelectCategoryCommand { get; }

        /// <summary>
        /// Gets command for creating a new post.
        /// </summary>
        public ICommand CreatePostCommand { get; }

        /// <summary>
        /// Handles navigation selection changes.
        /// </summary>
        /// <param name="args">The event arguments containing navigation details.</param>
        public void HandleNavigationSelectionChanged(NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not NavigationViewItem selectedItem || string.IsNullOrEmpty(selectedItem.Tag?.ToString()))
            {
                return;
            }

            if (selectedItem.MenuItems.Count > 0)
            {
                return;
            }

            var tag = selectedItem.Tag?.ToString();

            if (!string.IsNullOrEmpty(tag) && this.IsCategoryTag(tag))
            {
                this.HandleCategorySelection(tag);
            }
            else if (!string.IsNullOrEmpty(tag))
            {
                this.HandlePageNavigation(tag);
            }
        }

        /// <summary>
        /// Handles the result of a post creation operation.
        /// </summary>
        /// <param name="success">Indicates whether the post creation succeeded.</param>
        public void HandlePostCreation(bool success)
        {
            if (success && !string.IsNullOrEmpty(this.CurrentCategoryName))
            {
                // Notify the view to refresh the content
                this.PostCreationSucceeded?.Invoke(this, true);

                // Navigate back to the category to refresh the posts
                this.CategoryNavigationRequested?.Invoke(this, this.CurrentCategoryName);
            }
        }

        /// <summary>
        /// Initializes the ViewModel asynchronously.
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                this.IsLoading = true;
                await this.GetUserInfoAsync();
                await this.categoryViewModel.LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Initialization failed: {ex.Message}");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Retrieves user information asynchronously.
        /// </summary>
        private async Task GetUserInfoAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    User currentUser = this.userService.GetCurrentUser();
                    this.Username = currentUser.UserName;
                });
            }
            catch (Exception ex)
            {
                this.Username = "Guest";
                Debug.WriteLine($"Failed to get username: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the selection of a category.
        /// </summary>
        /// <param name="categoryName">The name of the selected category.</param>
        private async void HandleCategorySelection(string categoryName)
        {
            this.CurrentCategoryName = categoryName;
            try
            {
                var category = await this.categoryService.GetCategoryByName(categoryName);
                if (category != null)
                {
                    this.CurrentCategoryId = category.Id;
                    this.CategoryNavigationRequested?.Invoke(this, categoryName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting category ID: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles navigation to a specific page based on the provided tag.
        /// </summary>
        /// <param name="tag">The tag identifying the page to navigate to.</param>
        private void HandlePageNavigation(string tag)
        {
            this.CurrentCategoryName = string.Empty;
            this.CurrentCategoryId = 0;
            if (tag == "MainPage")
            {
                this.NavigationRequested?.Invoke(this, typeof(Views.Pages.MainPage));
            }
            else
            {
                Debug.WriteLine($"Unknown page tag: {tag}");
            }
        }

        /// <summary>
        /// Selects a category by its name.
        /// </summary>
        /// <param name="category">The name of the category to select.</param>
        private async void SelectCategory(string category)
        {
            if (this.IsCategoryTag(category))
            {
                this.CurrentCategoryName = category;
                try
                {
                    var categoryObj = await this.categoryService.GetCategoryByName(category);
                    if (categoryObj != null)
                    {
                        this.CurrentCategoryId = categoryObj.Id;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting category ID: {ex.Message}");
                }

                this.CategoryNavigationRequested?.Invoke(this, category);
            }
        }

        /// <summary>
        /// Determines whether the provided tag corresponds to a valid category.
        /// </summary>
        /// <param name="tag">The tag to check.</param>
        /// <returns>True if the tag is a valid category; otherwise, false.</returns>
        private bool IsCategoryTag(string tag)
        {
            return this.CategoryNames.Contains(tag);
        }

        /// <summary>
        /// Handles the creation of a new post.
        /// </summary>
        private void CreatePost()
        {
            // This is just a placeholder as the actual post creation is handled via the dialog
            // The view will handle showing the dialog when this command is executed
        }
    }
}