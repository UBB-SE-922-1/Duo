// <copyright file="ManageQuizesPage.xaml.cs" company="DuoISS">
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
    /// Page for managing quizzes and their exercises.
    /// </summary>
    public sealed partial class ManageQuizesPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManageQuizesPage"/> class.
        /// </summary>
        public ManageQuizesPage()
        {
            try
            {
                this.InitializeComponent();
                this.ViewModel.ShowListViewModal += this.ViewModel_openSelectExercises;
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
        /// <param name="e">The event data.</param>
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
                System.Diagnostics.Debug.WriteLine($"ViewModel_openSelectExercises error: {ex.Message}");
            }
        }
    }
}