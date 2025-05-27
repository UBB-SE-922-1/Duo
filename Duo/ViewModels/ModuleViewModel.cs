// <copyright file="ModuleViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Services;
    using DuoClassLibrary.Services.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// ViewModel for handling module-related logic, including completion, coin balance, and image click actions.
    /// </summary>
    public partial class ModuleViewModel : BaseViewModel, IModuleViewModel
    {
        private readonly ICourseService courseService;
        private readonly ICoinsService coinsService;
        private readonly ICourseViewModel courseViewModel;
        private int coinBalance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleViewModel"/> class.
        /// </summary>
        /// <param name="module">The module to display.</param>
        /// <param name="courseVM">The parent course view model.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="courseServiceOverride">Optional course service override.</param>
        /// <param name="coinsServiceOverride">Optional coins service override.</param>
        public ModuleViewModel(Module module, ICourseViewModel courseVM, int userId,
            ICourseService? courseServiceOverride = null,
            ICoinsService? coinsServiceOverride = null)
        {
            this.UserId = userId;

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:7174")
            };

            this.courseService = courseServiceOverride ?? new CourseService(new CourseServiceProxy(httpClient));
            this.coinsService = coinsServiceOverride ?? new CoinsService(new CoinsServiceProxy(httpClient));
            var userService = App.ServiceProvider.GetRequiredService<IUserService>();

            _ = this.EnsureUserExistsAsync(userService);

            this.CurrentModule = module;
            this.courseViewModel = courseVM;

            this.CompleteModuleCommand = new RelayCommand(this.ExecuteCompleteModule, this.CanCompleteModule);
            this.ModuleImageClickCommand = new RelayCommand(this.HandleModuleImageClick);

            _ = this.courseService.OpenModuleAsync(this.UserId, module.ModuleId);
            _ = this.InitializeAsync();
        }

        /// <summary>
        /// Gets the current module being displayed or worked on.
        /// </summary>
        public Module CurrentModule { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current module has been completed.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Gets the command that triggers the completion of the current module.
        /// </summary>
        public ICommand CompleteModuleCommand { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the command for handling actions related to the module's image click.
        /// </summary>
        public ICommand ModuleImageClickCommand { get; set; }

        /// <summary>
        /// Gets the formatted time spent on the current module.
        /// </summary>
        public string TimeSpent => this.courseViewModel.FormattedTimeRemaining;

        /// <summary>
        /// Gets the current coin balance related to the module.
        /// </summary>
        public int CoinBalance
        {
            get => this.coinBalance;
            private set
            {
                this.coinBalance = value;
                this.OnPropertyChanged(nameof(this.CoinBalance));
            }
        }

        /// <summary>
        /// Initializes the module view model asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            try
            {
                var userService = App.ServiceProvider.GetRequiredService<IUserService>();
                await this.EnsureUserExistsAsync(userService);

                this.IsCompleted = await this.courseService.IsModuleCompletedAsync(this.UserId, this.CurrentModule.ModuleId);
                await this.courseService.OpenModuleAsync(this.UserId, this.CurrentModule.ModuleId);

                this.courseViewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(this.courseViewModel.FormattedTimeRemaining))
                    {
                        this.OnPropertyChanged(nameof(this.TimeSpent));
                    }
                };
                this.OnPropertyChanged(nameof(this.IsCompleted));

                await this.LoadCoinBalanceAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in InitializeAsync: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the image click event for the current module, performing necessary actions.
        /// </summary>
        /// <param name="obj">The event parameter or context object.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleModuleImageClick(object? obj)
        {
            try
            {
                var confirmStatus = await this.courseService.ClickModuleImageAsync(this.UserId, this.CurrentModule.ModuleId);
                if (confirmStatus)
                {
                    this.OnPropertyChanged(nameof(this.CoinBalance));
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ClickModuleImage failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads and updates the coin balance asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadCoinBalanceAsync()
        {
            this.CoinBalance = await this.coinsService.GetCoinBalanceAsync(this.UserId);
        }

        private bool CanCompleteModule(object? parameter)
        {
            return !this.IsCompleted;
        }

        private void ExecuteCompleteModule(object? parameter)
        {
            this.courseViewModel.MarkModuleAsCompletedAndCheckRewards(this.CurrentModule.ModuleId, this.UserId);
            this.IsCompleted = true;
            this.OnPropertyChanged(nameof(this.IsCompleted));
            this.courseViewModel.RefreshCourseModulesDisplay(this.UserId);
        }

        /// <summary>
        /// Executes the module image click logic, triggering associated actions and UI updates.
        /// </summary>
        /// <param name="obj">The event parameter or context object.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteModuleImageClick(object? obj)
        {
            var success = await this.courseService.ClickModuleImageAsync(this.UserId, this.CurrentModule.ModuleId);
            if (success)
            {
                this.OnPropertyChanged(nameof(this.CoinBalance));
                this.courseViewModel.RefreshCourseModulesDisplay(this.UserId);
            }
        }

        private async Task EnsureUserExistsAsync(IUserService userService)
        {
            try
            {
                var user = await userService.GetUserById(this.UserId);
                if (user == null)
                {
                    Console.WriteLine($"User with ID {this.UserId} not found. Creating...");
                    // await userService.CreateUserAsync(new DuoClassLibrary.Models.User(this.UserId, $"DefaultUser{this.UserId}"));
                }
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine($"User with ID {this.UserId} not found (exception). Creating...");
                // await userService.CreateUserAsync(new DuoClassLibrary.Models.User(this.UserId, $"DefaultUser{this.UserId}"));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error in EnsureUserExistsAsync: {ex.Message}");
            }
        }
    }
}