// <copyright file="AdminMainPage.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using Duo.Views.Components.Modals;
    using DuoClassLibrary.Models.Exercises;
    using DuoClassLibrary.Models.Quizzes;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// Admin main page for managing sections, exercises, quizzes, and exams.
    /// </summary>
    public sealed partial class AdminMainPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminMainPage"/> class.
        /// </summary>
        public AdminMainPage()
        {
            this.InitializeComponent();

            // Subscribe to modal events
            this.ManageSectionsCard.AddButtonClicked += this.CreateSection_Click;
            this.ManageExercisesCard.AddButtonClicked += this.CreateExercise_Click;
            this.ManageQuizesCard.AddButtonClicked += this.CreateQuiz_Click;
            this.ManageExamsCard.AddButtonClicked += this.CreateExam_Click;

            this.ManageSectionsCard.ManageButtonClicked += this.OpenManageSectionsPage_Click;
            this.ManageExercisesCard.ManageButtonClicked += this.OpenManageExercisesPage_Click;
            this.ManageQuizesCard.ManageButtonClicked += this.OpenManageQuizesPage_Click;
            this.ManageExamsCard.ManageButtonClicked += this.OpenManageExamsPage_Click;
        }

        /// <summary>
        /// Navigates to the manage exercises page.
        /// </summary>
        public void OpenManageExercisesPage_Click(object? sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(ManageExercisesPage));
        }

        /// <summary>
        /// Navigates to the manage quizzes page.
        /// </summary>
        public void OpenManageQuizesPage_Click(object? sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(ManageQuizesPage));
        }

        /// <summary>
        /// Navigates to the manage sections page.
        /// </summary>
        public void OpenManageSectionsPage_Click(object? sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(ManageSectionsPage));
        }

        /// <summary>
        /// Navigates to the manage exams page.
        /// </summary>
        public void OpenManageExamsPage_Click(object? sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(ManageExamsPage));
        }

        private void CreateSection_Click(object? sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(CreateSectionPage));
        }

        private void CreateQuiz_Click(object? sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(CreateQuizPage));
        }

        private void CreateExam_Click(object? sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(CreateExamPage));
        }

        private void CreateExercise_Click(object? sender, EventArgs e)
        {
            this.Frame.Navigate(typeof(CreateExercisePage));
        }
    }
}