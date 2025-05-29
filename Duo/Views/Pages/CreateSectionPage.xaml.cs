// <copyright file="CreateSectionPage.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DuoClassLibrary.Models.Quizzes;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A page for creating sections.
    /// </summary>
    public sealed partial class CreateSectionPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSectionPage"/> class.
        /// </summary>
        public CreateSectionPage()
        {
            this.InitializeComponent();
            this.Loaded += this.RunAfterLoaded;
        }

        /// <summary>
        /// Handles the ViewModel's request to navigate back.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        public void ViewModelRequestGoBack(object sender, EventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void RunAfterLoaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ShowListViewModalQuizes += this.ViewModel_openSelectQuizes;
            this.ViewModel.ShowListViewModalExams += this.ViewModel_openSelectExams;
            this.ViewModel.ShowErrorMessageRequested += this.ViewModel_ShowErrorMessageRequested;
            this.ViewModel.RequestGoBack += this.ViewModelRequestGoBack;
        }

        private async void ViewModel_ShowErrorMessageRequested(object sender, (string Title, string Message) e)
        {
            await this.ShowErrorMessage(e.Title, e.Message);
        }

        private async Task ShowErrorMessage(string title, string message)
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

        private async void ViewModel_openSelectExams(List<Exam> exams)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Select Exam",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot,
                };

                var listView = new ListView
                {
                    ItemsSource = exams,
                    SelectionMode = ListViewSelectionMode.Single,
                    MaxHeight = 300,
                    ItemTemplate = (DataTemplate)this.Resources["ExamSelectionItemTemplate"],
                };

                dialog.Content = listView;
                dialog.PrimaryButtonText = "Add";
                dialog.IsPrimaryButtonEnabled = false;

                listView.SelectionChanged += (s, args) =>
                {
                    dialog.IsPrimaryButtonEnabled = listView.SelectedItem != null;
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary && listView.SelectedItem is Exam selectedExam)
                {
                    this.ViewModel.AddExam(selectedExam);
                }
            }
            catch (Exception ex)
            {
                await this.ShowErrorMessage("Selection Error", $"Failed to add exam: {ex.Message}");
            }
        }

        private async void ViewModel_openSelectQuizes(List<Quiz> quizzes)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Select Quiz",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot,
                };

                var listView = new ListView
                {
                    ItemsSource = quizzes,
                    SelectionMode = ListViewSelectionMode.Single,
                    MaxHeight = 300,
                    ItemTemplate = (DataTemplate)Resources["QuizSelectionItemTemplate"],
                };

                dialog.Content = listView;
                dialog.PrimaryButtonText = "Add";
                dialog.IsPrimaryButtonEnabled = false;

                listView.SelectionChanged += (s, args) =>
                {
                    dialog.IsPrimaryButtonEnabled = listView.SelectedItem != null;
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary && listView.SelectedItem is Quiz selectedQuiz)
                {
                    this.ViewModel.AddQuiz(selectedQuiz);
                }
            }
            catch (Exception ex)
            {
                await this.ShowErrorMessage("Selection Error", $"Failed to add quiz: {ex.Message}");
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