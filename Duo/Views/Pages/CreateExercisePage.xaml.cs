// <copyright file="CreateExercisePage.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A page for creating exercises.
    /// </summary>
    public sealed partial class CreateExercisePage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateExercisePage"/> class.
        /// </summary>
        public CreateExercisePage()
        {
            try
            {
                this.InitializeComponent();
                this.ViewModel.RequestGoBack += this.ViewModel_RequestGoBack;
                this.ViewModel.ShowErrorMessageRequested += this.ViewModel_ShowErrorMessageRequested;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Initialization error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the back button click event.
        /// </summary>
        public void BackButton_Click(object sender, RoutedEventArgs e)
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
                Debug.WriteLine($"BackButton_Click error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the cancel button click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e"> The event data.</param>
        public void CancelButton_Click(object sender, RoutedEventArgs e)
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
                Debug.WriteLine($"CancelButton_Click error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the ViewModel's request to go back.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e"> The event data.</param>
        public void ViewModel_RequestGoBack(object? sender, EventArgs e)
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
                Debug.WriteLine($"ViewModel_RequestGoBack error: {ex.Message}");
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
                Debug.WriteLine($"Failed to show error dialog: {ex.Message}");
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
                Debug.WriteLine($"ContentDialog error: {ex.Message}");
            }
        }
    }
}