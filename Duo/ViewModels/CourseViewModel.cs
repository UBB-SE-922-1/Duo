// <copyright file="CourseViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.Helpers.Interfaces;
    using Duo.Helpers.Timers;
    using Duo.ViewModels.Helpers;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Services;
    using DuoClassLibrary.Services.Interfaces;

    /// <summary>
    /// ViewModel for handling course presentation, progress tracking, and user interactions.
    /// </summary>
    public partial class CourseViewModel : BaseViewModel, ICourseViewModel
    {
        // Constants
        internal const int NotificationDisplayDurationInSeconds = 3;
        private const int CourseCompletionRewardCoins = 50;
        private const int TimedCompletionRewardCoins = 300;
        private const int TimeTrackingDatabaseAdjustmentDivisor = 2;
        private const int MinutesInAnHour = 60;

        // Fields
        private readonly ICourseService courseService;
        private readonly ICoinsService coinsService;
        private IDispatcherTimerHelper? courseProgressTimer;
        private INotificationHelper? notificationHelper;
        private int totalSecondsSpentOnCourse;
        private int courseCompletionTimeLimitInSeconds;
        private string? formattedTimeRemaining;
        private int lastSavedTimeInSeconds = 0;
        private string notificationMessageText = string.Empty;
        private bool shouldShowNotification = false;
        private int currentUserId;
        private bool isEnrolled;
        private int coinBalance;
        private ObservableCollection<Tag> tags = new();
        private bool isCourseTimerRunning;

        /// <summary>
        /// Gets the current user ID.
        /// </summary>
        public int CurrentUserId => this.currentUserId;

        /// <inheritdoc/>
        public Course CurrentCourse { get; }

        /// <inheritdoc/>
        public ObservableCollection<ModuleProgressStatus> ModuleRoadmap { get; } = new();

        /// <inheritdoc/>
        public ICommand? EnrollCommand { get; private set; }

        /// <inheritdoc/>
        public bool IsEnrolled
        {
            get => this.isEnrolled;
            set
            {
                if (this.isEnrolled != value)
                {
                    this.isEnrolled = value;
                    this.OnPropertyChanged(nameof(this.IsEnrolled));
                    if (this.EnrollCommand is RelayCommand cmd)
                    {
                        cmd.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public bool CoinVisibility => this.CurrentCourse.IsPremium && !this.IsEnrolled;

        /// <inheritdoc/>
        public int CoinBalance
        {
            get => this.coinBalance;
            set
            {
                if (this.coinBalance != value)
                {
                    this.coinBalance = value;
                    this.OnPropertyChanged(nameof(this.CoinBalance));
                    if (this.EnrollCommand is RelayCommand cmd)
                    {
                        cmd.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public ObservableCollection<Tag> Tags
        {
            get => this.tags;
            private set
            {
                this.tags = value;
                this.OnPropertyChanged();
            }
        }

        /// <inheritdoc/>
        public string FormattedTimeRemaining
        {
            get => this.formattedTimeRemaining!;
            private set
            {
                this.formattedTimeRemaining = value;
                this.OnPropertyChanged();
            }
        }

        /// <inheritdoc/>
        public virtual string NotificationMessage
        {
            get => this.notificationMessageText;
            set
            {
                this.notificationMessageText = value;
                this.OnPropertyChanged(nameof(this.NotificationMessage));
            }
        }

        /// <inheritdoc/>
        public virtual bool ShowNotification
        {
            get => this.shouldShowNotification;
            set
            {
                this.shouldShowNotification = value;
                this.OnPropertyChanged(nameof(this.ShowNotification));
            }
        }

        /// <inheritdoc/>
        public int CompletedModules { get; private set; }

        /// <inheritdoc/>
        public int RequiredModules { get; private set; }

        /// <inheritdoc/>
        public bool IsCourseCompleted => this.CompletedModules == this.RequiredModules;

        /// <inheritdoc/>
        public int TimeLimit { get; private set; }

        /// <inheritdoc/>
        public int TimeRemaining => Math.Max(0, this.TimeLimit - this.totalSecondsSpentOnCourse);

        /// <inheritdoc/>
        public bool CompletionRewardClaimed { get; private set; }

        /// <inheritdoc/>
        public bool TimedRewardClaimed { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the course timer is running.
        /// </summary>
        internal bool IsCourseTimerRunning
        {
            get => this.isCourseTimerRunning;
            set => this.isCourseTimerRunning = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseViewModel"/> class.
        /// </summary>
        public CourseViewModel()
        {
            this.CurrentCourse = new Course
            {
                Title = string.Empty,
                Description = string.Empty,
                ImageUrl = string.Empty,
                Difficulty = string.Empty,
            };

            var httpClient = new System.Net.Http.HttpClient();
            var courseServiceProxy = new CourseServiceProxy(httpClient);

            this.courseService = new CourseService(courseServiceProxy);
            this.coinsService = new CoinsService(new CoinsServiceProxy(httpClient));
            this.notificationHelper = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseViewModel"/> class.
        /// </summary>
        /// <param name="course">The course to display and manage.</param>
        /// <param name="currentUserId">The current user ID.</param>
        /// <param name="courseService">The service for course-related operations (optional).</param>
        /// <param name="coinsService">The service for coin-related operations (optional).</param>
        /// <param name="timerService">The timer service for course progress tracking (optional).</param>
        /// <param name="notificationTimerService">The timer service for notifications (optional).</param>
        /// <param name="notificationHelper">The notification helper (optional).</param>
        /// <param name="serviceProxy">The course service proxy (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown when course is null.</exception>
        public CourseViewModel(
            Course course,
            int currentUserId,
            ICourseService? courseService = null,
            ICoinsService? coinsService = null,
            IDispatcherTimerHelper? timerService = null,
            IDispatcherTimerHelper? notificationTimerService = null,
            INotificationHelper? notificationHelper = null,
            CourseServiceProxy? serviceProxy = null)
        {
            this.CurrentCourse = course ?? throw new ArgumentNullException(nameof(course));
            this.currentUserId = currentUserId;

            var httpClient = new System.Net.Http.HttpClient();
            var defaultServiceProxy = serviceProxy ?? new CourseServiceProxy(httpClient);

            this.courseService = courseService ?? new CourseService(defaultServiceProxy);
            this.coinsService = coinsService ?? new CoinsService(new CoinsServiceProxy(httpClient));

            this.EnrollCommand = new RelayCommand(
                async (_) => await this.EnrollUserInCourseAsync(_, currentUserId),
                (_) => !this.IsEnrolled && (this.CurrentCourse.Cost == 0 || this.CoinBalance >= this.CurrentCourse.Cost));

            this.InitializeTimersAndNotificationHelper(timerService, notificationTimerService, notificationHelper);
        }

        /// <inheritdoc/>
        public async Task<int> GetCoinBalanceAsync(int currentUserId)
        {
            try
            {
                this.CoinBalance = await this.coinsService.GetCoinBalanceAsync(currentUserId);
                return this.CoinBalance;
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Failed to load coin balance", e.Message);
                return 0;
            }
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(int currentUserId)
        {
            try
            {
                await this.InitializeProperties(currentUserId);
                await this.LoadInitialData(currentUserId);
                await this.LoadTagsAsync();
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Initialization Error", $"Failed to initialize course data.\nDetails: {ex.Message}");
            }
        }

        private async Task LoadTagsAsync()
        {
            try
            {
                var tagList = await this.courseService.GetCourseTagsAsync(this.CurrentCourse.CourseId);
                this.Tags = new ObservableCollection<Tag>(tagList);
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Failed to load tags", e.Message);
            }
        }

        private void InitializeTimersAndNotificationHelper(
            IDispatcherTimerHelper? timerService,
            IDispatcherTimerHelper? notificationTimerService,
            INotificationHelper? notificationHelper)
        {
            this.courseProgressTimer = timerService ?? new DispatcherTimerHelper();
            var notificationTimer = notificationTimerService ?? new DispatcherTimerHelper();
            this.notificationHelper = notificationHelper ?? new NotificationHelper(this, notificationTimer);
            this.courseProgressTimer.Tick += this.OnCourseTimerTick;
        }

        private async Task InitializeProperties(int currentUserId)
        {
            try
            {
                this.IsEnrolled = await this.courseService.IsUserEnrolledAsync(currentUserId, this.CurrentCourse.CourseId);
                this.CoinBalance = await this.coinsService.GetCoinBalanceAsync(currentUserId);

                this.EnrollCommand = new RelayCommand(
                    async (_) => await this.EnrollUserInCourseAsync(_, currentUserId),
                    (_) => !this.IsEnrolled && (this.CurrentCourse.Cost == 0 || this.CoinBalance >= this.CurrentCourse.Cost));

                this.OnPropertyChanged(nameof(this.EnrollCommand));
                this.OnPropertyChanged(nameof(this.IsEnrolled));
                this.OnPropertyChanged(nameof(this.CoinBalance));
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Initialization Error", $"Failed to check enrollment.\nDetails: {ex.Message}");
            }
        }

        private async Task LoadInitialData(int currentUserId)
        {
            try
            {
                this.totalSecondsSpentOnCourse = await this.courseService.GetTimeSpentAsync(currentUserId, this.CurrentCourse.CourseId);
                this.lastSavedTimeInSeconds = this.totalSecondsSpentOnCourse;
                this.courseCompletionTimeLimitInSeconds = this.CurrentCourse.TimeToComplete - this.totalSecondsSpentOnCourse;
                this.FormattedTimeRemaining = FormatTimeRemainingDisplay(this.courseCompletionTimeLimitInSeconds - this.totalSecondsSpentOnCourse);

                this.CompletedModules = await this.courseService.GetCompletedModulesCountAsync(currentUserId, this.CurrentCourse.CourseId);
                this.RequiredModules = await this.courseService.GetRequiredModulesCountAsync(this.CurrentCourse.CourseId);
                this.TimeLimit = await this.courseService.GetCourseTimeLimitAsync(this.CurrentCourse.CourseId);

                await this.LoadAndOrganizeCourseModules(currentUserId);
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Course Load Error", $"Unable to load course data.\nDetails: {ex.Message}");
            }
        }

        private void OnCourseTimerTick(object? sender, EventArgs e)
        {
            try
            {
                this.totalSecondsSpentOnCourse++;
                this.UpdateTimeDisplay();
                this.OnPropertyChanged(nameof(this.TimeRemaining));
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Timer Error", $"An error occurred while updating the timer.\nDetails: {ex.Message}");
            }
        }

        private void UpdateTimeDisplay()
        {
            try
            {
                int remainingSeconds = this.courseCompletionTimeLimitInSeconds - this.totalSecondsSpentOnCourse;
                this.FormattedTimeRemaining = FormatTimeRemainingDisplay(Math.Max(0, remainingSeconds));
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Display Update Error", $"Failed to update remaining time.\nDetails: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        public async Task LoadAndOrganizeCourseModules(int currentUserId)
        {
            try
            {
                var modules = await this.courseService.GetModulesAsync(this.CurrentCourse.CourseId);
                if (modules == null || modules.Count == 0)
                {
                    Console.WriteLine("No modules found, skipping module display.");
                    return;
                }

                this.ModuleRoadmap.Clear();

                for (int index = 0; index < modules.Count; index++)
                {
                    var module = modules[index];

                    bool isCompleted = false;
                    bool isUnlocked = false;

                    try
                    {
                        isCompleted = await this.courseService.IsModuleCompletedAsync(currentUserId, module.ModuleId);
                        isUnlocked = await this.GetModuleUnlockStatus(module, index, currentUserId);
                    }
                    catch (KeyNotFoundException kex)
                    {
                        Console.WriteLine($"Module {module.ModuleId} failed: {kex.Message}");
                        continue;
                    }

                    this.ModuleRoadmap.Add(new ModuleProgressStatus
                    {
                        Module = module,
                        IsUnlocked = isUnlocked,
                        IsCompleted = isCompleted,
                    });
                }

                this.OnPropertyChanged(nameof(this.ModuleRoadmap));
            }
            catch (Exception e)
            {
                Console.WriteLine($"LoadAndOrganizeCourseModules crashed: {e.Message}");
                this.RaiseErrorMessage("Loading Modules Failed", $"Could not load modules.\nDetails: {e.Message}");
            }
        }

        private async Task<bool> GetModuleUnlockStatus(Module module, int moduleIndex, int currentUserId)
        {
            try
            {
                if (!module.IsBonus)
                {
                    return this.IsEnrolled &&
                        (moduleIndex == 0 ||
                        await this.courseService.IsModuleCompletedAsync(currentUserId, this.ModuleRoadmap[moduleIndex - 1].Module!.ModuleId));
                }

                return await this.courseService.IsModuleInProgressAsync(currentUserId, module.ModuleId);
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Module Unlock Error", $"Failed to check unlock status.\nDetails: {e.Message}");
                return false;
            }
        }

        private async Task<bool> CanUserEnrollInCourseAsync(object? parameter, int currentUserId)
        {
            try
            {
                int coinBalance = await this.GetCoinBalanceAsync(currentUserId);
                return !this.IsEnrolled && coinBalance >= this.CurrentCourse.Cost;
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Enrollment Check Failed", $"Unable to verify enrollment eligibility.\nDetails: {e.Message}");
                return false;
            }
        }

        private async Task EnrollUserInCourseAsync(object? parameter, int currentUserId)
        {
            try
            {
                if (this.CurrentCourse.Cost > 0)
                {
                    bool coinDeductionSuccessful = await this.coinsService.TrySpendingCoinsAsync(currentUserId, this.CurrentCourse.Cost);
                    if (!coinDeductionSuccessful)
                    {
                        this.RaiseErrorMessage("Insufficient Coins", "You do not have enough coins to enroll.");
                        return;
                    }
                }

                bool enrollmentSuccessful = await this.courseService.EnrollInCourseAsync(currentUserId, this.CurrentCourse.CourseId);
                if (!enrollmentSuccessful)
                {
                    this.RaiseErrorMessage("Enrollment Failed", "Unable to enroll in the course.");
                    return;
                }

                this.CoinBalance = await this.coinsService.GetCoinBalanceAsync(currentUserId);
                this.IsEnrolled = true;
                this.ResetCourseProgressTracking();
                this.OnPropertyChanged(nameof(this.IsEnrolled));
                this.OnPropertyChanged(nameof(this.CoinBalance));

                this.StartCourseProgressTimer();
                await this.LoadAndOrganizeCourseModules(currentUserId);
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Enrollment Error", $"Failed to enroll in the course.\nDetails: {e.Message}");
            }
        }

        private void ResetCourseProgressTracking()
        {
            try
            {
                this.totalSecondsSpentOnCourse = 0;
                this.FormattedTimeRemaining = FormatTimeRemainingDisplay(this.totalSecondsSpentOnCourse);
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Reset Progress Error", $"Failed to reset course progress.\nDetails: {e.Message}");
            }
        }

        /// <summary>
        /// Starts the course progress timer if not already running.
        /// </summary>
        public void StartCourseProgressTimer()
        {
            try
            {
                if (!this.IsCourseTimerRunning && this.IsEnrolled)
                {
                    this.IsCourseTimerRunning = true;
                    this.courseProgressTimer!.Start();
                }
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Timer Start Error", $"Could not start the course timer.\nDetails: {e.Message}");
            }
        }

        /// <summary>
        /// Pauses the course progress timer and saves the current progress.
        /// </summary>
        /// <param name="currentUserId">The current user ID.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PauseCourseProgressTimer(int currentUserId)
        {
            try
            {
                if (this.IsCourseTimerRunning)
                {
                    this.courseProgressTimer!.Stop();
                    await this.SaveCourseProgressTime(currentUserId);
                    this.IsCourseTimerRunning = false;
                }
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Pause Timer Error", $"Could not pause and save the timer.\nDetails: {e.Message}");
            }
        }

        private async Task SaveCourseProgressTime(int currentUserId)
        {
            try
            {
                int secondsToSave = (this.totalSecondsSpentOnCourse - this.lastSavedTimeInSeconds) / TimeTrackingDatabaseAdjustmentDivisor;

                Console.WriteLine($"Attempting to save: Current={this.totalSecondsSpentOnCourse}, LastSaved={this.lastSavedTimeInSeconds}, ToSave={secondsToSave}");

                if (secondsToSave > 0)
                {
                    Console.WriteLine($"Saving {secondsToSave} seconds");
                    await this.courseService.UpdateTimeSpentAsync(currentUserId, this.CurrentCourse.CourseId, secondsToSave);
                    this.lastSavedTimeInSeconds = this.totalSecondsSpentOnCourse;
                }
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Save Time Error", $"Could not save course progress.\nDetails: {e.Message}");
            }
        }

        /// <summary>
        /// Formats time in seconds to a display string (X min Y sec).
        /// </summary>
        /// <param name="totalSeconds">Total seconds to format.</param>
        /// <returns>Formatted time string.</returns>
        internal static string FormatTimeRemainingDisplay(int totalSeconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
            int totalMinutes = timeSpan.Minutes + (timeSpan.Hours * MinutesInAnHour);
            return $"{totalMinutes} min {timeSpan.Seconds} sec";
        }

        /// <summary>
        /// Refreshes the course modules display.
        /// </summary>
        /// <param name="currentUserId">The current user ID.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RefreshCourseModulesDisplay(int currentUserId)
        {
            try
            {
                await this.LoadAndOrganizeCourseModules(currentUserId);
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Module Refresh Error", $"Unable to refresh modules.\nDetails: {e.Message}");
            }
        }

        /// <summary>
        /// Marks a module as completed and checks for any earned rewards.
        /// </summary>
        /// <param name="targetModuleId">ID of the module to mark as completed.</param>
        /// <param name="currentUserId">The current user ID.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task MarkModuleAsCompletedAndCheckRewards(int targetModuleId, int currentUserId)
        {
            try
            {
                await this.courseService.CompleteModuleAsync(currentUserId, targetModuleId, this.CurrentCourse.CourseId);

                Module completedModule = await this.courseService.GetModuleAsync(targetModuleId);
                if (!completedModule.IsBonus)
                {
                    await this.UpdateCompletionStatus(currentUserId);

                    if (this.IsCourseCompleted)
                    {
                        await this.CheckForCompletionReward(currentUserId);
                        await this.CheckForTimedReward(currentUserId);
                    }
                }
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Module Completion Error", $"Failed to mark module as completed.\nDetails: {e.Message}");
            }
        }

        private async Task UpdateCompletionStatus(int currentUserId)
        {
            try
            {
                this.CompletedModules = await this.courseService.GetCompletedModulesCountAsync(currentUserId, this.CurrentCourse.CourseId);
                this.OnPropertyChanged(nameof(this.CompletedModules));
                this.OnPropertyChanged(nameof(this.IsCourseCompleted));
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Status Update Error", $"Failed to update module completion status.\nDetails: {e.Message}");
            }
        }

        private async Task CheckForCompletionReward(int currentUserId)
        {
            try
            {
                bool rewardClaimed = await this.courseService.ClaimCompletionRewardAsync(currentUserId, this.CurrentCourse.CourseId, CourseCompletionRewardCoins);
                if (rewardClaimed)
                {
                    this.CompletionRewardClaimed = true;
                    this.OnPropertyChanged(nameof(this.CompletionRewardClaimed));
                    this.OnPropertyChanged(nameof(this.CoinBalance));
                    this.ShowCourseCompletionRewardNotification();
                }
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Completion Reward Error", $"Failed to claim course completion reward.\nDetails: {e.Message}");
            }
        }

        private async Task CheckForTimedReward(int currentUserId)
        {
            try
            {
                if (this.TimeRemaining > 0)
                {
                    bool rewardClaimed = await this.courseService.ClaimTimedRewardAsync(currentUserId, this.CurrentCourse.CourseId, this.totalSecondsSpentOnCourse, TimedCompletionRewardCoins);
                    if (rewardClaimed)
                    {
                        this.TimedRewardClaimed = true;
                        this.OnPropertyChanged(nameof(this.TimedRewardClaimed));
                        this.OnPropertyChanged(nameof(this.CoinBalance));
                        this.ShowTimedCompletionRewardNotification();
                    }
                }
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Timed Reward Error", $"Failed to claim timed reward.\nDetails: {e.Message}");
            }
        }

        private void ShowCourseCompletionRewardNotification()
        {
            try
            {
                string message = $"Congratulations! You have completed all required modules in this course. {CourseCompletionRewardCoins} coins have been added to your balance.";
                this.notificationHelper!.ShowTemporaryNotification(message);
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Notification Error", $"Failed to show course completion reward notification.\nDetails: {e.Message}");
            }
        }

        private void ShowTimedCompletionRewardNotification()
        {
            try
            {
                string message = $"Congratulations! You completed the course within the time limit. {TimedCompletionRewardCoins} coins have been added to your balance.";
                this.notificationHelper!.ShowTemporaryNotification(message);
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Notification Error", $"Failed to show timed completion reward notification.\nDetails: {e.Message}");
            }
        }

        private async Task ShowModulePurchaseNotificationAsync(Module module, int currentUserId)
        {
            try
            {
                string message = $"Congratulations! You have purchased bonus module {module.Title}, {module.Cost} coins have been deducted from your balance.";
                this.notificationHelper!.ShowTemporaryNotification(message);
                await this.RefreshCourseModulesDisplay(currentUserId);
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Purchase Notification Error", $"Failed to show purchase success notification.\nDetails: {e.Message}");
            }
        }

        /// <summary>
        /// Attempts to purchase a bonus module.
        /// </summary>
        /// <param name="module">The module to purchase.</param>
        /// <param name="currentUserId">The current user ID.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AttemptBonusModulePurchaseAsync(Module? module, int currentUserId)
        {
            ArgumentNullException.ThrowIfNull(module);

            try
            {
                if (await this.courseService.IsModuleCompletedAsync(currentUserId, module.ModuleId))
                {
                    return;
                }

                bool purchaseSuccessful = await this.courseService.BuyBonusModuleAsync(currentUserId, module.ModuleId);

                if (purchaseSuccessful)
                {
                    await this.UpdatePurchasedModuleStatus(module, currentUserId);
                    await this.ShowModulePurchaseNotificationAsync(module, currentUserId);
                    this.CoinBalance = await this.coinsService.GetCoinBalanceAsync(currentUserId);
                    this.OnPropertyChanged(nameof(this.ModuleRoadmap));
                    this.OnPropertyChanged(nameof(this.CoinBalance));
                }
                else
                {
                    this.ShowPurchaseFailedNotification();
                }
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Purchase Error", $"Could not complete purchase of module '{module.Title}'.\nDetails: {e.Message}");
            }
        }

        private async Task UpdatePurchasedModuleStatus(Module module, int currentUserId)
        {
            try
            {
                var moduleToUpdate = this.ModuleRoadmap.FirstOrDefault(m => m.Module!.ModuleId == module.ModuleId);
                if (moduleToUpdate != null)
                {
                    moduleToUpdate.IsUnlocked = true;
                    moduleToUpdate.IsCompleted = false;
                    try
                    {
                        await this.courseService.OpenModuleAsync(currentUserId, module.ModuleId);
                    }
                    catch (KeyNotFoundException ex)
                    {
                        Console.WriteLine($"OpenModuleAsync failed: {ex.Message}");
                        this.notificationHelper?.ShowTemporaryNotification("Something went wrong. Please try again.");
                    }
                }
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Update Error", $"Failed to update the status of purchased module '{module.Title}'.\nDetails: {e.Message}");
            }
        }

        private void ShowPurchaseFailedNotification()
        {
            try
            {
                this.notificationHelper!.ShowTemporaryNotification("You do not have enough coins to buy this module.");
            }
            catch (Exception e)
            {
                this.RaiseErrorMessage("Notification Error", $"Failed to show failed purchase notification.\nDetails: {e.Message}");
            }
        }

        /// <summary>
        /// Represents a module along with its progress status.
        /// </summary>
        public class ModuleProgressStatus
        {
            /// <summary>
            /// Gets or sets the module.
            /// </summary>
            public Module? Module { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the module is unlocked.
            /// </summary>
            public bool IsUnlocked { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the module is completed.
            /// </summary>
            public bool IsCompleted { get; set; }

            /// <summary>
            /// Gets a value indicating whether the module is a locked bonus.
            /// </summary>
            public bool IsLockedBonus => this.Module?.IsBonus == true && !this.IsUnlocked;
        }
    }
}