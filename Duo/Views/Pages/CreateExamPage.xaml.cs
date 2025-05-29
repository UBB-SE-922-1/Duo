// <copyright file="CreateExamPage.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DuoClassLibrary.Models.Exercises;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// Page for creating a new exam and selecting exercises.
    /// </summary>
    public sealed partial class CreateExamPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateExamPage"/> class.
        /// </summary>
        public CreateExamPage()
        {
            this.InitializeComponent();
            this.Loaded += this.CreateExamPage_Loaded;
        }

        /// <summary>
        /// Handles navigation back when requested by the ViewModel.
        /// </summary>
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
                _ = this.ShowErrorMessage("Navigation Error", $"Failed to navigate back.\nDetails: {ex.Message}");
            }
        }

        private void CreateExamPage_Loaded(object? sender, RoutedEventArgs e)
        {
            this.ViewModel.ShowListViewModal += this.ViewModel_openSelectExercises;
            this.ViewModel.RequestGoBack += this.ViewModel_RequestGoBack;
            this.ViewModel.ShowErrorMessageRequested += this.ViewModel_ShowErrorMessageRequested;
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
                // Optionally log this or suppress it to avoid recursive dialog errors
                Console.WriteLine($"Error dialog failed to display. Details: {ex.Message}");
            }
        }

        private async void ViewModel_openSelectExercises(List<Exercise> exercises)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Select Exercise",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot,
                };

                var listView = new ListView
                {
                    ItemsSource = exercises,
                    SelectionMode = ListViewSelectionMode.Single,
                    MaxHeight = 300,
                    ItemTemplate = (DataTemplate)this.Resources["ExerciseSelectionItemTemplate"],
                };

                dialog.Content = listView;
                dialog.PrimaryButtonText = "Add";
                dialog.IsPrimaryButtonEnabled = false;

                listView.SelectionChanged += (s, args) =>
                {
                    dialog.IsPrimaryButtonEnabled = listView.SelectedItem != null;
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary && listView.SelectedItem is Exercise selectedExercise)
                {
                    this.ViewModel.AddExercise(selectedExercise);
                }
            }
            catch (Exception ex)
            {
                await this.ShowErrorMessage("Dialog Error", $"Failed to open exercise selection dialog.\nDetails: {ex.Message}");
            }
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
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
                _ = this.ShowErrorMessage("Navigation Error", $"Failed to cancel and go back.\nDetails: {ex.Message}");
            }
        }
    }
}