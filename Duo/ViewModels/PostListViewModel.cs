// <copyright file="PostListViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.Services;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Services.Interfaces;
    using Microsoft.UI.Xaml;

    /// <summary>
    /// ViewModel for listing and filtering posts with pagination and hashtag support.
    /// </summary>
    public class PostListViewModel : INotifyPropertyChanged
    {
        // Constants for validation and defaults
        private const int InvalidId = 0;
        private const int DefaultItemsPerPage = 5;
        private const int DefaultPageNumber = 1;
        private const int DefaultTotalPages = 1;
        private const int DefaultCount = 0;
        private const string AllHashtagsFilter = "All";
        private const int ItemsPerPage = 5;

        private readonly IPostService postService;

        private int? categoryId;
        private string categoryName = string.Empty;
        private string filterText = string.Empty;
        private ObservableCollection<Post> posts;
        private int currentPage;
        private HashSet<string> selectedHashtags = new HashSet<string>();
        private int totalPages = 1;
        private List<string> allHashtags = new List<string>();
        private int totalPostCount = 0;
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostListViewModel"/> class.
        /// </summary>
        /// <param name="postService">The post service.</param>
        /// <param name="categoryService">The category service (optional).</param>
        public PostListViewModel(IPostService postService, ICategoryService? categoryService = null)
        {
            this.postService = postService ?? App.PostService;
            this.posts = new ObservableCollection<Post>();
            this.currentPage = DefaultPageNumber;
            this.selectedHashtags.Add(AllHashtagsFilter);

            this.LoadPostsCommand = new RelayCommand(async _ => await this.LoadPosts());
            this.NextPageCommand = new RelayCommand(async _ => await this.NextPage());
            this.PreviousPageCommand = new RelayCommand(async _ => await this.PreviousPage());
            this.FilterPostsCommand = new RelayCommand(async _ => await this.FilterPosts());
            this.ClearFiltersCommand = new RelayCommand(async _ => await this.ClearFilters());
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes the view model asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            await this.LoadAllHashtagsAsync();
            await this.LoadPosts();
        }

        /// <summary>
        /// Gets or sets the collection of posts.
        /// </summary>
        public ObservableCollection<Post> Posts
        {
            get => this.posts;
            set
            {
                this.posts = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int CurrentPage
        {
            get => this.currentPage;
            set
            {
                this.currentPage = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the filter text.
        /// </summary>
        public string FilterText
        {
            get => this.filterText;
            set
            {
                this.filterText = value;
                this.OnPropertyChanged();
                _ = this.FilterPosts();
            }
        }

        /// <summary>
        /// Gets or sets the category ID.
        /// </summary>
        public int CategoryID
        {
            get => this.categoryId ?? InvalidId;
            set
            {
                this.categoryId = value;
                this.OnPropertyChanged();
                _ = this.LoadAllHashtagsAsync();
            }
        }

        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string CategoryName
        {
            get => this.categoryName;
            set
            {
                this.categoryName = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages
        {
            get => this.totalPages;
            set
            {
                this.totalPages = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the view model is loading.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set
            {
                this.isLoading = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the selected hashtags.
        /// </summary>
        public HashSet<string> SelectedHashtags => this.selectedHashtags;

        /// <summary>
        /// Gets or sets all available hashtags.
        /// </summary>
        public List<string> AllHashtags
        {
            get => this.allHashtags;
            set
            {
                this.allHashtags = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the command to load posts.
        /// </summary>
        public ICommand LoadPostsCommand { get; }

        /// <summary>
        /// Gets the command to go to the next page.
        /// </summary>
        public ICommand NextPageCommand { get; }

        /// <summary>
        /// Gets the command to go to the previous page.
        /// </summary>
        public ICommand PreviousPageCommand { get; }

        /// <summary>
        /// Gets the command to filter posts.
        /// </summary>
        public ICommand FilterPostsCommand { get; }

        /// <summary>
        /// Gets the command to clear filters.
        /// </summary>
        public ICommand ClearFiltersCommand { get; }

        /// <summary>
        /// Loads posts asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task LoadPosts()
        {
            try
            {
                this.IsLoading = true;
                var result = await this.postService.GetFilteredAndFormattedPosts(
                    this.categoryId,
                    this.selectedHashtags.ToList(),
                    this.filterText,
                    this.currentPage,
                    ItemsPerPage);

                var posts = result.Posts;
                var totalCount = result.TotalCount;

                this.Posts.Clear();
                foreach (var post in posts)
                {
                    this.Posts.Add(post);
                }

                this.totalPostCount = totalCount;
                this.TotalPages = Math.Max(DefaultTotalPages, (int)Math.Ceiling(this.totalPostCount / (double)ItemsPerPage));
                this.OnPropertyChanged(nameof(this.TotalPages));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading posts: {ex.Message}");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Toggles a hashtag filter and reloads posts.
        /// </summary>
        /// <param name="hashtag">The hashtag to toggle.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ToggleHashtag(string hashtag)
        {
            this.selectedHashtags = this.postService.ToggleHashtagSelection(this.selectedHashtags, hashtag, AllHashtagsFilter);
            this.CurrentPage = DefaultPageNumber;
            this.OnPropertyChanged(nameof(this.SelectedHashtags));
            await this.LoadPosts();
        }

        /// <summary>
        /// Filters posts based on the current filter text.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FilterPosts()
        {
            this.CurrentPage = DefaultPageNumber;
            await this.LoadPosts();
        }

        /// <summary>
        /// Clears all filters and reloads posts.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ClearFilters()
        {
            this.FilterText = string.Empty;
            this.selectedHashtags.Clear();
            this.selectedHashtags.Add(AllHashtagsFilter);
            this.CurrentPage = DefaultPageNumber;
            await this.LoadPosts();
            this.OnPropertyChanged(nameof(this.SelectedHashtags));
        }

        /// <summary>
        /// Loads all hashtags asynchronously.
        /// </summary>
        private async Task LoadAllHashtagsAsync()
        {
            try
            {
                this.allHashtags.Clear();
                this.allHashtags.Add(AllHashtagsFilter);

                var hashtags = await this.postService.GetHashtags(this.categoryId);
                foreach (var hashtag in hashtags)
                {
                    if (!this.allHashtags.Contains(hashtag.Tag))
                    {
                        this.allHashtags.Add(hashtag.Tag);
                    }
                }

                this.OnPropertyChanged(nameof(this.AllHashtags));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading hashtags: {ex.Message}");
            }
        }

        /// <summary>
        /// Goes to the next page.
        /// </summary>
        private async Task NextPage()
        {
            if (this.CurrentPage < this.TotalPages)
            {
                this.CurrentPage++;
                await this.LoadPosts();
            }
        }

        /// <summary>
        /// Goes to the previous page.
        /// </summary>
        private async Task PreviousPage()
        {
            if (this.CurrentPage > DefaultPageNumber)
            {
                this.CurrentPage--;
                await this.LoadPosts();
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}