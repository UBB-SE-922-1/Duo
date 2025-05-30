// <copyright file="RoadmapMainPage.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Threading.Tasks;
    using Duo.ViewModels.Base;
    using Duo.ViewModels.Roadmap;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    /// <summary>
    /// The main roadmap page for the application.
    /// </summary>
    public sealed partial class RoadmapMainPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoadmapMainPage"/> class.
        /// </summary>
        public RoadmapMainPage()
        {
            try
            {
                this.InitializeComponent();
                if (this.ViewModel is ViewModelBase viewModel)
                {
                    viewModel.ShowErrorMessageRequested += this.ViewModel_ShowErrorMessageRequested;
                }
                else
                {
                    _ = this.ShowErrorMessage("Initialization Error", "ViewModel is not set to a valid ViewModelBase instance.");
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Initialization Error", $"Failed to initialize RoadmapMainPage.\nDetails: {ex.Message}");
            }
        }

        /// <inheritdoc/>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (this.ViewModel is RoadmapMainPageViewModel viewModel)
                {
                    await viewModel.SetupViewModel();
                }
                else
                {
                    _ = this.ShowErrorMessage("Navigation Error", "ViewModel is not set to a valid RoadmapMainPageViewModel.");
                }

                base.OnNavigatedTo(e);
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Navigation Error", $"Failed to set up RoadmapMainPage.\nDetails: {ex.Message}");
            }
        }

        private async void ViewModel_ShowErrorMessageRequested(object? sender, (string Title, string Message) e)
        {
            await this.ShowErrorMessage(e.Title, e.Message);
        }

        private async Task ShowErrorMessage(string title, string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error dialog failed to display. Details: {ex.Message}");
            }
        }
    }
}