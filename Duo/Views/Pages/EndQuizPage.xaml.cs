// <copyright file="EndQuizPage.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Threading.Tasks;
    using DuoClassLibrary.Models.Quizzes;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;

    /// <summary>
    /// A page that displays the results at the end of a quiz.
    /// </summary>
    public sealed partial class EndQuizPage : Page
    {
        private readonly Quiz quiz;
        private readonly TimeSpan timeTaken;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndQuizPage"/> class.
        /// </summary>
        /// <param name="quiz">The quiz object.</param>
        /// <param name="timeTaken">The time taken to complete the quiz.</param>
        public EndQuizPage(Quiz quiz, TimeSpan timeTaken)
        {
            this.InitializeComponent();
            this.quiz = quiz;
            this.timeTaken = timeTaken;
            this.DisplayResults();
        }

        private void DisplayResults()
        {
            double scorePercentage = ((double)this.quiz.GetNumberOfCorrectAnswers() / this.quiz.GetNumberOfAnswersGiven()) * 100;

            this.ScoreTextBlock.Text = $"{this.quiz.GetNumberOfCorrectAnswers()}/{this.quiz.GetNumberOfAnswersGiven()} ({scorePercentage:F1}%)";
            this.TimeTextBlock.Text = $"{this.timeTaken.Minutes}m {this.timeTaken.Seconds}s";

            if (scorePercentage >= this.quiz.GetPassingThreshold())
            {
                this.FeedbackTextBlock.Text = "Great job! You've passed the quiz!";
                this.FeedbackTextBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
            }
            else
            {
                this.FeedbackTextBlock.Text = "Keep practicing! You can do better next time.";
                this.FeedbackTextBlock.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red);
            }
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.Frame.CanGoBack)
                {
                    this.Frame.GoBack();
                }
                else
                {
                    this.Frame.Navigate(typeof(RoadmapMainPage));
                }
            }
            catch (Exception ex)
            {
                await this.ShowErrorMessage("Navigation Error", ex.Message);
            }
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
    }
}