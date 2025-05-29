// <copyright file="QuizEndScreen.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using Duo.Views.Pages;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A user control that displays the end screen for a quiz.
    /// </summary>
    public sealed partial class QuizEndScreen : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuizEndScreen"/> class.
        /// </summary>
        public QuizEndScreen()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the text displaying the number of correct answers.
        /// </summary>
        public string CorrectAnswersText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text displaying the passing percent.
        /// </summary>
        public string PassingPercentText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the text indicating if the quiz was passed.
        /// </summary>
        public string IsPassedText { get; set; } = string.Empty;

        private void GoToRoadmap_Click(object sender, RoutedEventArgs e)
        {
            var parentFrame = Helpers.Helpers.FindParent<Frame>(this);
            if (parentFrame != null)
            {
                parentFrame.Navigate(typeof(RoadmapMainPage));
            }
        }
    }
}