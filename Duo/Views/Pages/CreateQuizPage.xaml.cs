// <copyright file="CreateQuizPage.xaml.cs" company="DuoISS">
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
    /// A page for creating quizzes.
    /// </summary>
    public sealed partial class CreateQuizPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQuizPage"/> class.
        /// </summary>
        public CreateQuizPage()
        {
            this.InitializeComponent();
            this.ViewModel.ShowListViewModal += exercises =>
            {
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    this.ViewModel_openSelectExercises(exercises);
                });
            };
            this.ViewModel.RequestGoBack += this.ViewModel_RequestGoBack;
            this.ViewModel.ShowErrorMessageRequested += this.ViewModel_ShowErrorMessageRequested;
        }

        /// <summary>
        /// Handles the ViewModel's request to go back.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        public void ViewModel_RequestGoBack(object? sender, EventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
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
                System.Diagnostics.Debug.WriteLine($"Failed to show error dialog: {ex.Message}");
            }
        }

        private async void ViewModel_openSelectExercises(List<Exercise> exercises)
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}