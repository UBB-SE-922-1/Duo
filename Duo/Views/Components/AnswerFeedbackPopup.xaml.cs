// <copyright file="AnswerFeedbackPopup.xaml.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.UI;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;

    /// <summary>
    /// A dialog that displays feedback for correct or incorrect answers.
    /// </summary>
    public sealed partial class AnswerFeedbackPopup : ContentDialog
    {
        private readonly SolidColorBrush greenBrush = new SolidColorBrush(Colors.Green);
        private readonly SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        /// <summary>
        /// Initializes a new instance of the <see cref="AnswerFeedbackPopup"/> class.
        /// </summary>
        public AnswerFeedbackPopup()
        {
            try
            {
                this.InitializeComponent();
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Initialization Error", $"Failed to initialize AnswerFeedbackPopup.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows the feedback for a correct answer.
        /// </summary>
        /// <param name="correctAnswer">The correct answer text.</param>
        public void ShowCorrectAnswer(string correctAnswer)
        {
            try
            {
                this.FeedbackIcon.Glyph = "\uE73E"; // Checkmark icon
                this.FeedbackIcon.Foreground = this.greenBrush;
                this.FeedbackMessage.Text = "Correct! Well done!";
                this.FeedbackMessage.Foreground = this.greenBrush;
                this.CorrectAnswerText.Text = correctAnswer;
                this.CloseButton.Background = this.greenBrush;
                this.CloseButton.Foreground = new SolidColorBrush(Colors.White);
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Show Correct Answer Error", $"Failed to display correct answer feedback.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows the feedback for a wrong answer.
        /// </summary>
        /// <param name="correctAnswer">The correct answer text.</param>
        public void ShowWrongAnswer(string correctAnswer)
        {
            try
            {
                this.FeedbackIcon.Glyph = "\uE783"; // X icon
                this.FeedbackIcon.Foreground = this.redBrush;
                this.FeedbackMessage.Text = "Incorrect. Keep trying!";
                this.FeedbackMessage.Foreground = this.redBrush;
                this.CorrectAnswerText.Text = correctAnswer;
                this.CloseButton.Background = this.redBrush;
                this.CloseButton.Foreground = new SolidColorBrush(Colors.White);
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Show Wrong Answer Error", $"Failed to display wrong answer feedback.\nDetails: {ex.Message}");
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
                Console.WriteLine($"Error dialog failed to display. Details: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Hide();
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Close Dialog Error", $"Failed to close feedback dialog.\nDetails: {ex.Message}");
            }
        }
    }
}