// <copyright file="PostListPage.xaml.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Duo.Services;
    using Duo.ViewModels;
    using DuoClassLibrary.Models;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Input;
    using Microsoft.UI.Xaml.Navigation;
    using static Duo.App;

    /// <summary>
    /// Displays a list of posts with filtering, pagination, and hashtag support.
    /// </summary>
    public sealed partial class PostListPage : Page
    {
        private const int InvalidId = 0;
        private const int DefaultPageNumber = 1;

        public PostListViewModel viewModel;
        private readonly Dictionary<string, Button> hashtagButtons = new Dictionary<string, Button>();
        private double previousPosition;
        private bool isDragging;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostListPage"/> class.
        /// </summary>
        public PostListPage()
        {
            this.InitializeComponent();

            var postService = App.PostService;
            var categoryService = App.CategoryService;

            this.viewModel = new PostListViewModel(postService, categoryService);

            this.DataContext = this.viewModel;

            this.PostsPager.SelectedIndexChanged += this.PostsPager_SelectedIndexChanged;
            this.FilterByTitle.TextChanged += this.OnFilterChanged;

            this.SetupHashtagDragScrolling();
        }

        /// <inheritdoc/>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string categoryName && !string.IsNullOrEmpty(categoryName))
            {
                this.viewModel.CategoryName = categoryName;
                this.PageTitle.Text = categoryName;

                try
                {
                    var categoryInfo = await CategoryService.GetCategoryByName(categoryName);
                    if (categoryInfo != null)
                    {
                        if (categoryName == "Community")
                        {
                            this.viewModel.CategoryID = 0;
                        }
                        else
                        {
                            this.viewModel.CategoryID = categoryInfo.Id;
                        }

                        System.Diagnostics.Debug.WriteLine($"CategoryID: {this.viewModel.CategoryID}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting category: {ex.Message}");
                }
            }

            await this.viewModel.InitializeAsync();
            this.UpdateHashtagsList();
        }

        private void UpdateHashtagsList()
        {
            this.HashtagsContainer.Items.Clear();
            this.hashtagButtons.Clear();

            if (this.viewModel.AllHashtags != null)
            {
                foreach (var hashtag in this.viewModel.AllHashtags)
                {
                    Button button = new Button
                    {
                        Content = hashtag == "All" ? "All" : $"#{hashtag}",
                        Tag = hashtag,
                        Style = this.viewModel.SelectedHashtags.Contains(hashtag)
                            ? this.Resources["SelectedHashtagButtonStyle"] as Style
                            : this.Resources["HashtagButtonStyle"] as Style,
                    };

                    button.Click += this.Hashtag_Click;
                    this.HashtagsContainer.Items.Add(button);
                    this.hashtagButtons[hashtag] = button;
                }
            }
        }

        private async void PostsPager_SelectedIndexChanged(PipsPager sender, PipsPagerSelectedIndexChangedEventArgs args)
        {
            this.viewModel.CurrentPage = sender.SelectedPageIndex + DefaultPageNumber;
            await this.viewModel.LoadPosts();
        }

        private void OnFilterChanged(object sender, TextChangedEventArgs args)
        {
            this.viewModel.FilterText = this.FilterByTitle.Text;
        }

        private async void Hashtag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string hashtag)
            {
                await this.viewModel.ToggleHashtag(hashtag);

                this.UpdateHashtagButtonStyles();

                this.PostsPager.NumberOfPages = this.viewModel.TotalPages;
                this.PostsPager.SelectedPageIndex = this.viewModel.CurrentPage - 1;
            }
        }

        private void UpdateHashtagButtonStyles()
        {
            foreach (var entry in this.hashtagButtons)
            {
                string hashtag = entry.Key;
                Button button = entry.Value;

                button.Style = this.viewModel.SelectedHashtags.Contains(hashtag)
                    ? this.Resources["SelectedHashtagButtonStyle"] as Style
                    : this.Resources["HashtagButtonStyle"] as Style;
            }
        }

        private async void ClearHashtags_Click(object sender, RoutedEventArgs e)
        {
            await this.viewModel.ClearFilters();
            this.UpdateHashtagButtonStyles();

            this.PostsPager.NumberOfPages = this.viewModel.TotalPages;
            this.PostsPager.SelectedPageIndex = this.viewModel.CurrentPage - 1;
        }

        private void FilteredListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Post clickedPost)
            {
                this.Frame.Navigate(typeof(PostDetailPage), clickedPost);
            }
        }

        private void SetupHashtagDragScrolling()
        {
            this.HashtagsScrollViewer.PointerPressed += this.HashtagsScrollViewer_PointerPressed;
            this.HashtagsScrollViewer.PointerMoved += this.HashtagsScrollViewer_PointerMoved;
            this.HashtagsScrollViewer.PointerReleased += this.HashtagsScrollViewer_PointerReleased;
            this.HashtagsScrollViewer.PointerExited += this.HashtagsScrollViewer_PointerReleased;
            this.HashtagsScrollViewer.PointerCaptureLost += this.HashtagsScrollViewer_PointerReleased;
        }

        private void HashtagsScrollViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.isDragging = true;
            this.previousPosition = e.GetCurrentPoint(this.HashtagsScrollViewer).Position.X;

            this.HashtagsScrollViewer.CapturePointer(e.Pointer);

            e.Handled = true;
        }

        private void HashtagsScrollViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (this.isDragging)
            {
                var currentPosition = e.GetCurrentPoint(this.HashtagsScrollViewer).Position.X;
                var delta = this.previousPosition - currentPosition;

                this.HashtagsScrollViewer.ChangeView(this.HashtagsScrollViewer.HorizontalOffset + delta, null, null);

                this.previousPosition = currentPosition;
                e.Handled = true;
            }
        }

        private void HashtagsScrollViewer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (this.isDragging)
            {
                this.isDragging = false;
                this.HashtagsScrollViewer.ReleasePointerCapture(e.Pointer);
                e.Handled = true;
            }
        }
    }
}