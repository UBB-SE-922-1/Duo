// <copyright file="ManageExercisesPage.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// Page for managing exercises.
    /// </summary>
    public sealed partial class ManageExercisesPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManageExercisesPage"/> class.
        /// </summary>
        public ManageExercisesPage()
        {
            try
            {
                this.InitializeComponent();
                this.ViewModel.ShowErrorMessageRequested += this.ViewModel_ShowErrorMessageRequested;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Initialization error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the back button click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e"> The event data.</param>
        public void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (this.Frame.CanGoBack)
                {
                    this.Frame.GoBack();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BackButton_Click error: {ex.Message}");
            }
        }

        private async void ViewModel_ShowErrorMessageRequested(object? sender, (string Title, string Message) e)
        {
            try
            {
                await this.ShowErrorMessage(e.Title, e.Message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to show error dialog: {ex.Message}");
            }
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
                System.Diagnostics.Debug.WriteLine($"ContentDialog error: {ex.Message}");
            }
        }
    }
}