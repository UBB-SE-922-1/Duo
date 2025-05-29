// <copyright file="RoadmapQuizPreview.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Duo.ViewModels.Base;
    using Duo.ViewModels.Roadmap;
    using Duo.Views.Pages;
    using DuoClassLibrary.Models.Quizzes;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A user control for previewing a quiz or exam in the roadmap.
    /// </summary>
    public sealed partial class RoadmapQuizPreview : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RoadmapQuizPreview"/> class.
        /// </summary>
        public RoadmapQuizPreview()
        {
            try
            {
                this.InitializeComponent();
                if (this.DataContext is ViewModelBase viewModel)
                {
                    viewModel.ShowErrorMessageRequested += this.ViewModel_ShowErrorMessageRequested;
                }
                else
                {
                    _ = this.ShowErrorMessage("Initialization Error", "DataContext is not set to a valid ViewModel.");
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Initialization Error", $"Failed to initialize RoadmapQuizPreview.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the click event to open a quiz or exam.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        public void OpenQuizButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button && this.DataContext is RoadmapQuizPreviewViewModel viewModel)
                {
                    var parentFrame = Helpers.Helpers.FindParent<Frame>(this);
                    if (parentFrame != null)
                    {
                        if (viewModel.Quiz is Exam)
                        {
                            Debug.WriteLine("Navigating to Exam");
                            parentFrame.Navigate(typeof(QuizPage), (viewModel.Quiz.Id, true));
                        }
                        else
                        {
                            Debug.WriteLine("Navigating to Quiz");
                            parentFrame.Navigate(typeof(QuizPage), (viewModel.Quiz.Id, false));
                        }
                    }
                    else
                    {
                        _ = this.ShowErrorMessage("Navigation Error", "Failed to find parent frame for navigation.");
                    }
                }
                else
                {
                    _ = this.ShowErrorMessage("Click Error", "Invalid button or ViewModel type.");
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Navigation Error", $"Failed to navigate to quiz page.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the quiz or exam preview.
        /// </summary>
        /// <param name="quizId">The quiz or exam ID.</param>
        /// <param name="isExam">Whether the item is an exam.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task Load(int quizId, bool isExam)
        {
            try
            {
                if (this.DataContext is RoadmapQuizPreviewViewModel viewModel)
                {
                    await viewModel.OpenForQuiz(quizId, isExam);
                }
                else
                {
                    _ = this.ShowErrorMessage("Load Error", "DataContext is not set to a valid ViewModel.");
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Load Error", $"Failed to load quiz with ID {quizId}.\nDetails: {ex.Message}");
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