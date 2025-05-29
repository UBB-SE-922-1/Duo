// <copyright file="RoadmapSectionView.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Duo.ViewModels.Base;
    using Duo.Views.Pages;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A user control for displaying a section in the roadmap.
    /// </summary>
    public sealed partial class RoadmapSectionView : UserControl
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="RoadmapSectionView"/> class.
        /// </summary>
        public RoadmapSectionView()
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

        /// <summary>
        /// Occurs when the section is clicked.
        /// </summary>
        public event RoutedEventHandler? Click;

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

        private void Quiz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is QuizRoadmapButton button)
                {
                    Debug.WriteLine($"Quiz with ID {button.QuizId} clicked!");

                    var parentFrame = Helpers.Helpers.FindParent<Frame>(this);
                    if (parentFrame != null)
                    {
                        parentFrame.Navigate(typeof(QuizPreviewPage), (button.QuizId, button.IsExam));
                    }
                    else
                    {
                        _ = this.ShowErrorMessage("Navigation Error", "Failed to find parent frame for navigation.");
                    }
                }
                else
                {
                    _ = this.ShowErrorMessage("Click Error", "Invalid button type clicked.");
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Navigation Error", $"Failed to navigate to quiz preview.\nDetails: {ex.Message}");
            }
        }
    }
}