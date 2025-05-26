// <copyright file="PostCreationViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.Services;
    using Duo.Views.Components;
    using DuoClassLibrary.Helpers;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Services.Interfaces;
    using static Duo.App;

    /// <summary>
    /// The PostCreationViewModel is responsible for managing the creation and editing of posts.
    /// It provides properties and methods for interacting with the post creation UI,
    /// and handles the communication with the database through the PostService.
    /// Features:
    /// - Managing post title and content.
    /// - Handling hashtags (add, remove).
    /// - Community selection.
    /// - Post creation with validation.
    /// - Error handling.
    /// </summary>
    public class PostCreationViewModel : INotifyPropertyChanged
    {
        // Constants for validation and defaults
        private const int InvalidId = 0;
        private const int DefaultCount = 0;
        private const string EmptyString = "";

        // Services
        private readonly IPostService postService;
        private readonly ICategoryService categoryService;
        private readonly IUserService userService;

        // Properties
        private string postTitle = string.Empty;
        private string postContent = string.Empty;
        private int selectedCategoryId;
        private ObservableCollection<string> postHashtags = new ObservableCollection<string>();
        private ObservableCollection<CommunityItem> postCommunities = new ObservableCollection<CommunityItem>();
        private string lastError = string.Empty;
        private bool isLoading;
        private bool isSuccess;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Occurs when post creation is successful.
        /// </summary>
        public event EventHandler? PostCreationSuccessful;

        /// <summary>
        /// Gets the command to create a post.
        /// </summary>
        public ICommand CreatePostCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a hashtag.
        /// </summary>
        public ICommand AddHashtagCommand { get; private set; }

        /// <summary>
        /// Gets the command to remove a hashtag.
        /// </summary>
        public ICommand RemoveHashtagCommand { get; private set; }

        /// <summary>
        /// Gets the command to select a community.
        /// </summary>
        public ICommand SelectCommunityCommand { get; private set; }

        /// <summary>
        /// Gets or sets the post title.
        /// </summary>
        public string Title
        {
            get => this.postTitle;
            set
            {
                if (this.postTitle != value)
                {
                    this.postTitle = value;
                    var (isValid, errorMessage) = ValidationHelper.ValidatePostTitle(value);
                    if (!isValid)
                    {
                        this.LastError = errorMessage;
                    }
                    else
                    {
                        this.LastError = EmptyString;
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the post content.
        /// </summary>
        public string Content
        {
            get => this.postContent;
            set
            {
                if (this.postContent != value)
                {
                    this.postContent = value;
                    var (isValid, errorMessage) = ValidationHelper.ValidatePostContent(value);
                    if (!isValid)
                    {
                        this.LastError = errorMessage;
                    }
                    else
                    {
                        this.LastError = EmptyString;
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected category ID.
        /// </summary>
        public int SelectedCategoryId
        {
            get => this.selectedCategoryId;
            set
            {
                if (this.selectedCategoryId != value)
                {
                    this.selectedCategoryId = value;
                    this.OnPropertyChanged();
                    this.UpdateSelectedCommunity();
                }
            }
        }

        /// <summary>
        /// Gets the collection of hashtags.
        /// </summary>
        public ObservableCollection<string> Hashtags => this.postHashtags;

        /// <summary>
        /// Gets the collection of communities.
        /// </summary>
        public ObservableCollection<CommunityItem> Communities => this.postCommunities;

        /// <summary>
        /// Gets or sets the last error message.
        /// </summary>
        public string LastError
        {
            get => this.lastError;
            set
            {
                if (this.lastError != value)
                {
                    this.lastError = value;
                    this.OnPropertyChanged();
                }
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
                if (this.isLoading != value)
                {
                    this.isLoading = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess
        {
            get => this.isSuccess;
            set
            {
                if (this.isSuccess != value)
                {
                    this.isSuccess = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostCreationViewModel"/> class.
        /// </summary>
        public PostCreationViewModel()
        {
            this.postService = App.PostService;
            this.categoryService = App.CategoryService;
            this.userService = App.UserService;

            this.CreatePostCommand = new RelayCommand(async _ => await this.CreatePostAsync());
            this.AddHashtagCommand = new RelayCommandWithParameter<string>(this.AddHashtag);
            this.RemoveHashtagCommand = new RelayCommandWithParameter<string>(this.RemoveHashtag);
            this.SelectCommunityCommand = new RelayCommandWithParameter<int>(this.SelectCommunity);

            this.LoadCommunities();
        }

        /// <summary>
        /// Creates a post asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CreatePostAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Title) || string.IsNullOrWhiteSpace(this.Content))
            {
                this.LastError = "Title and content are required.";
                return;
            }

            var (isTitleValid, titleError) = ValidationHelper.ValidatePostTitle(this.Title);
            if (!isTitleValid)
            {
                this.LastError = titleError;
                return;
            }

            if (this.SelectedCategoryId <= InvalidId)
            {
                this.LastError = "Please select a community for your post.";
                return;
            }

            this.IsLoading = true;
            this.LastError = EmptyString;

            try
            {
                var currentUser = this.userService.GetCurrentUser();

                var newPost = new DuoClassLibrary.Models.Post
                {
                    Title = this.Title,
                    Description = this.Content,
                    UserID = currentUser.UserId,
                    CategoryID = this.SelectedCategoryId,
                    CreatedAt = DateTimeHelper.EnsureUtcKind(DateTime.UtcNow),
                    UpdatedAt = DateTimeHelper.EnsureUtcKind(DateTime.UtcNow),
                };

                int createdPostId = await this.postService.CreatePost(newPost);

                if (this.Hashtags.Count > DefaultCount)
                {
                    foreach (var hashtagText in this.Hashtags)
                    {
                        try
                        {
                            await this.postService.AddHashtagToPost(createdPostId, hashtagText, currentUser.UserId);
                        }
                        catch (Exception hashtagException)
                        {
                            Debug.WriteLine($"Error adding hashtag '{hashtagText}' to post: {hashtagException.Message}");

                            // Continue with other hashtags even if one fails
                        }
                    }
                }

                this.IsSuccess = true;
                this.PostCreationSuccessful?.Invoke(this, EventArgs.Empty);

                this.ClearForm();
            }
            catch (Exception postCreationException)
            {
                Debug.WriteLine($"Error creating post: {postCreationException.Message}");
                this.LastError = $"Failed to create post: {postCreationException.Message}";
                this.IsSuccess = false;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Creates a post asynchronously with parameters.
        /// </summary>
        /// <param name="title">The post title.</param>
        /// <param name="content">The post content.</param>
        /// <param name="categoryId">The category ID.</param>
        /// <param name="hashtags">The list of hashtags.</param>
        /// <returns>A task representing the asynchronous operation, with a result indicating success.</returns>
        public async Task<bool> CreatePostAsync(string title, string content, int categoryId, List<string>? hashtags = null)
        {
            this.Title = title;
            this.Content = content;
            this.SelectedCategoryId = categoryId;

            Debug.WriteLine($"CreatePostAsync - START - Title: '{title}', Content length: {content?.Length ?? 0}, CategoryID: {categoryId}");
            Debug.WriteLine($"CreatePostAsync - Received hashtags: {hashtags?.Count ?? 0}");
            if (hashtags != null && hashtags.Count > DefaultCount)
            {
                Debug.WriteLine($"CreatePostAsync - Hashtags to add: {string.Join(", ", hashtags)}");
            }

            List<string> processedHashtags = new List<string>();
            if (hashtags != null && hashtags.Count > DefaultCount)
            {
                foreach (var hashtagText in hashtags)
                {
                    if (!string.IsNullOrWhiteSpace(hashtagText))
                    {
                        processedHashtags.Add(hashtagText.Trim());
                    }
                }
            }

            this.IsLoading = true;
            this.LastError = EmptyString;

            try
            {
                var currentUser = this.userService.GetCurrentUser();

                var newPost = new DuoClassLibrary.Models.Post
                {
                    Title = this.Title,
                    Description = this.Content,
                    UserID = currentUser.UserId,
                    CategoryID = this.SelectedCategoryId,
                    CreatedAt = DateTimeHelper.EnsureUtcKind(DateTime.UtcNow),
                    UpdatedAt = DateTimeHelper.EnsureUtcKind(DateTime.UtcNow),
                };

                int createdPostId = await this.postService.CreatePostWithHashtags(newPost, processedHashtags, currentUser.UserId);

                this.Hashtags.Clear();
                foreach (var hashtagText in processedHashtags)
                {
                    this.postHashtags.Add(hashtagText);
                }

                this.IsSuccess = true;
                this.PostCreationSuccessful?.Invoke(this, EventArgs.Empty);

                return true;
            }
            catch (Exception postCreationException)
            {
                Debug.WriteLine($"Error creating post: {postCreationException.Message}");
                this.LastError = $"Failed to create post: {postCreationException.Message}";
                this.IsSuccess = false;
                return false;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Adds a hashtag to the collection.
        /// </summary>
        /// <param name="hashtag">The hashtag to add.</param>
        public void AddHashtag(string hashtag)
        {
            if (string.IsNullOrWhiteSpace(hashtag))
            {
                Debug.WriteLine("AddHashtag - Empty hashtag provided, ignoring");
                return;
            }

            string trimmedHashtag = hashtag.Trim();

            if (!this.Hashtags.Contains(trimmedHashtag))
            {
                this.postHashtags.Add(trimmedHashtag);

                Debug.WriteLine($"Added hashtag to ViewModel: {trimmedHashtag}, Count now: {this.Hashtags.Count}");

                this.OnPropertyChanged(nameof(this.Hashtags));
            }
            else
            {
                Debug.WriteLine($"Hashtag '{trimmedHashtag}' already exists in collection, not adding duplicate");
            }
        }

        /// <summary>
        /// Removes a hashtag from the collection.
        /// </summary>
        /// <param name="hashtag">The hashtag to remove.</param>
        public void RemoveHashtag(string hashtag)
        {
            if (!string.IsNullOrWhiteSpace(hashtag) && this.Hashtags.Contains(hashtag))
            {
                this.Hashtags.Remove(hashtag);

                Debug.WriteLine($"Removed hashtag from ViewModel: {hashtag}, Count now: {this.Hashtags.Count}");

                this.OnPropertyChanged(nameof(this.Hashtags));
            }
        }

        /// <summary>
        /// Selects a community by its ID.
        /// </summary>
        /// <param name="communityId">The community ID.</param>
        public void SelectCommunity(int communityId)
        {
            this.SelectedCategoryId = communityId;
        }

        /// <summary>
        /// Clears the form fields.
        /// </summary>
        public void ClearForm()
        {
            this.Title = EmptyString;
            this.Content = EmptyString;
            this.SelectedCategoryId = InvalidId;
            this.Hashtags.Clear();
            this.UpdateSelectedCommunity();
            this.LastError = EmptyString;
            this.IsSuccess = false;
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void LoadCommunities()
        {
            try
            {
                var allCategories = await this.categoryService.GetAllCategories();

                this.Communities.Clear();
                foreach (var category in allCategories)
                {
                    this.Communities.Add(new CommunityItem
                    {
                        Id = category.Id,
                        Name = category.Name,
                        IsSelected = category.Id == this.SelectedCategoryId,
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading communities: {ex.Message}");
                this.LastError = $"Failed to load communities: {ex.Message}";
            }
        }

        private void UpdateSelectedCommunity()
        {
            foreach (var community in this.Communities)
            {
                community.IsSelected = community.Id == this.SelectedCategoryId;
            }
        }
    }
}