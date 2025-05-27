// <copyright file="MainViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Services;
    using DuoClassLibrary.Services.Interfaces;

    /// <summary>
    /// ViewModel responsible for managing the main application logic, including course display, filtering, and user coin balance.
    /// </summary>
    public partial class MainViewModel : BaseViewModel, IMainViewModel
    {
        private int CurrentUserId { get; init; }

        private readonly ICourseService courseService;
        private readonly ICoinsService coinsService;
        private string searchQuery = string.Empty;
        private bool filterByPremium;
        private bool filterByFree;
        private bool filterByEnrolled;
        private bool filterByNotEnrolled;
        private ObservableCollection<Course> displayedCourses;
        private ObservableCollection<Tag> availableTags;
        private int userCoinBalance;
        private bool isUpdatingFiltersBatch = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="serviceProxy">The coins service proxy.</param>
        /// <param name="courseServiceProxy">The course service proxy.</param>
        /// <param name="currentUserId">The current user ID.</param>
        /// <param name="courseService">Optional course service instance.</param>
        /// <param name="coinsService">Optional coins service instance.</param>
        public MainViewModel(CoinsServiceProxy serviceProxy, CourseServiceProxy courseServiceProxy, int currentUserId,
            ICourseService? courseService = null, ICoinsService? coinsService = null)
        {
            this.CurrentUserId = currentUserId;
            this.courseService = courseService ?? new CourseService(courseServiceProxy);
            this.coinsService = coinsService ?? new CoinsService(serviceProxy);
            this.InitializeAsync();
        }

        /// <summary>
        /// Gets or sets the collection of courses to be displayed.
        /// </summary>
        public ObservableCollection<Course> DisplayedCourses
        {
            get => this.displayedCourses;
            set
            {
                if (this.displayedCourses != value)
                {
                    this.displayedCourses = value;
                    this.OnPropertyChanged(nameof(this.DisplayedCourses));
                }
            }
        }

        /// <summary>
        /// Gets or sets the collection of available tags.
        /// </summary>
        public ObservableCollection<Tag> AvailableTags
        {
            get => this.availableTags;
            set
            {
                if (this.availableTags != value)
                {
                    // Unsubscribe from old tags
                    if (this.availableTags != null)
                    {
                        foreach (var tag in this.availableTags)
                        {
                            tag.PropertyChanged -= this.Tag_PropertyChanged;
                        }
                    }

                    this.availableTags = value;
                    this.OnPropertyChanged(nameof(this.AvailableTags));

                    // Subscribe to new tags
                    if (this.availableTags != null)
                    {
                        foreach (var tag in this.availableTags)
                        {
                            tag.PropertyChanged += this.Tag_PropertyChanged;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the user's current coin balance.
        /// </summary>
        public int UserCoinBalance
        {
            get => this.userCoinBalance;
            private set
            {
                this.userCoinBalance = value;
                this.OnPropertyChanged(nameof(this.UserCoinBalance));
            }
        }

        /// <summary>
        /// Gets or sets the search query used to filter courses.
        /// </summary>
        public string SearchQuery
        {
            get => this.searchQuery;
            set
            {
                if (value.Length <= 100 && this.searchQuery != value)
                {
                    this.searchQuery = value;
                    this.OnPropertyChanged();
                    this.ApplyAllFilters();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to filter by premium courses.
        /// </summary>
        public bool FilterByPremium
        {
            get => this.filterByPremium;
            set
            {
                if (this.filterByPremium != value)
                {
                    this.filterByPremium = value;
                    this.OnPropertyChanged();
                    this.ApplyAllFilters();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to filter by free courses.
        /// </summary>
        public bool FilterByFree
        {
            get => this.filterByFree;
            set
            {
                if (this.filterByFree != value)
                {
                    this.filterByFree = value;
                    this.OnPropertyChanged();
                    this.ApplyAllFilters();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to filter by enrolled courses.
        /// </summary>
        public bool FilterByEnrolled
        {
            get => this.filterByEnrolled;
            set
            {
                if (this.filterByEnrolled != value)
                {
                    this.filterByEnrolled = value;
                    this.OnPropertyChanged();
                    this.ApplyAllFilters();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to filter by not enrolled courses.
        /// </summary>
        public bool FilterByNotEnrolled
        {
            get => this.filterByNotEnrolled;
            set
            {
                if (this.filterByNotEnrolled != value)
                {
                    this.filterByNotEnrolled = value;
                    this.OnPropertyChanged();
                    this.ApplyAllFilters();
                }
            }
        }

        /// <summary>
        /// Gets the command to reset all filters.
        /// </summary>
        public ICommand ResetAllFiltersCommand { get; private set; }

        /// <summary>
        /// Refreshes the user's coin balance asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RefreshUserCoinBalanceAsync()
        {
            try
            {
                this.UserCoinBalance = await this.coinsService.GetCoinBalanceAsync(this.CurrentUserId);
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Failed to refresh coin balance", ex.Message);
            }
        }

        private async void InitializeAsync()
        {
            try
            {
                this.ResetAllFiltersCommand = new RelayCommand(this.ResetAllFilters);

                var courseList = await this.courseService.GetCoursesAsync();
                foreach (var course in courseList)
                {
                    course.Tags = await this.courseService.GetCourseTagsAsync(course.CourseId);
                }
                this.DisplayedCourses = new ObservableCollection<Course>(courseList);
                this.AvailableTags = new ObservableCollection<Tag>(await this.courseService.GetTagsAsync());

                await this.RefreshUserCoinBalanceAsync();
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Failed to load courses", e.Message);
            }
        }

        /// <summary>
        /// Attempts to grant a daily login reward to the user.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result indicates if the reward was granted.</returns>
        public async Task<bool> TryDailyLoginReward()
        {
            try
            {
                bool loginRewardGranted = await this.coinsService.ApplyDailyLoginBonusAsync(this.CurrentUserId);
                await this.RefreshUserCoinBalanceAsync();
                return loginRewardGranted;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Daily login reward failed", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Resets all the filters and clears the search query.
        /// </summary>
        /// <param name="parameter">The command parameter (not used).</param>
        private void ResetAllFilters(object? parameter)
        {
            try
            {
                this.isUpdatingFiltersBatch = true;
                this.SearchQuery = string.Empty;
                this.FilterByPremium = false;
                this.FilterByFree = false;
                this.FilterByEnrolled = false;
                this.FilterByNotEnrolled = false;

                foreach (var tag in this.AvailableTags)
                {
                    tag.IsSelected = false;
                }

                this.isUpdatingFiltersBatch = false;
                this.ApplyAllFilters();
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Reset filters failed", ex.Message);
            }
        }

        /// <summary>
        /// Applies all filters based on search query, selected tags, and filter flags.
        /// </summary>
        private async void ApplyAllFilters()
        {
            try
            {
                if (this.AvailableTags == null)
                {
                    return;
                }

                var existingCourseTags = this.CacheExistingCourseTags();
                this.DisplayedCourses.Clear();
                await this.LoadFilteredCoursesWithTags(existingCourseTags);
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Failed to apply filters", e.Message);
            }
        }

        /// <summary>
        /// Saves the tags of currently displayed courses for reuse.
        /// </summary>
        /// <returns>A dictionary mapping course IDs to their tags.</returns>
        private Dictionary<int, List<Tag>> CacheExistingCourseTags()
        {
            var tagCache = new Dictionary<int, List<Tag>>();
            foreach (var course in this.DisplayedCourses)
            {
                if (course.Tags != null && course.Tags.Any())
                {
                    tagCache[course.CourseId] = course.Tags.ToList();
                }
            }

            return tagCache;
        }

        /// <summary>
        /// Loads filtered courses with their tags, using cached tags when available.
        /// </summary>
        /// <param name="tagCache">A dictionary mapping course IDs to their tags.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadFilteredCoursesWithTags(Dictionary<int, List<Tag>> tagCache)
        {
            var selectedTagIds = this.AvailableTags
                .Where(tag => tag.IsSelected)
                .Select(tag => tag.TagId)
                .ToList();

            var filteredCourses = await this.courseService.GetFilteredCoursesAsync(
                this.searchQuery,
                this.filterByPremium,
                this.filterByFree,
                this.filterByEnrolled,
                this.filterByNotEnrolled,
                selectedTagIds,
                this.CurrentUserId);

            foreach (var course in filteredCourses)
            {
                if (tagCache.ContainsKey(course.CourseId))
                {
                    course.Tags = tagCache[course.CourseId];
                }
                else if (course.Tags == null || !course.Tags.Any())
                {
                    course.Tags = await this.courseService.GetCourseTagsAsync(course.CourseId);
                }

                this.DisplayedCourses.Add(course);
            }
        }

        /// <summary>
        /// Handles property changes for tags to reapply filters when a tag's selection state changes.
        /// </summary>
        /// <param name="sender">The tag object.</param>
        /// <param name="e">The property changed event arguments.</param>
        private void Tag_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Tag.IsSelected))
            {
                if (!this.isUpdatingFiltersBatch)
                {
                    this.ApplyAllFilters();
                }
            }
        }

        /// <summary>
        /// Public method to reset all filters.
        /// </summary>
        public void ResetAllFiltersPublic()
        {
            this.ResetAllFilters(null);
        }
    }
}