// <copyright file="CreateSectionModal.xaml.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.Views.Components.Modals
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using DuoClassLibrary.Models.Quizzes;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// Modal dialog for creating a section.
    /// </summary>
    public sealed partial class CreateSectionModal : UserControl
    {
        /// <summary>
        /// Occurs when a section is created.
        /// </summary>
        public event EventHandler<SectionCreatedEventArgs>? SectionCreated;

        /// <summary>
        /// Occurs when the modal is closed.
        /// </summary>
        public event EventHandler? ModalClosed;

        private readonly List<Quiz> availableQuizzes;
        private readonly List<Exam> availableExams;

        /// <summary>
        /// Gets the collection of unassigned quizzes.
        /// </summary>
        public ObservableCollection<Quiz> UnassignedQuizzes { get; }

        /// <summary>
        /// Gets the collection of selected exams (should contain only one).
        /// </summary>
        public ObservableCollection<Exam> SelectedExam { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSectionModal"/> class.
        /// </summary>
        public CreateSectionModal()
        {
            this.InitializeComponent();

            this.availableQuizzes = new List<Quiz>
            {
                new Quiz(1, null, null),
                new Quiz(2, null, null),
                new Quiz(3, null, null),
                new Quiz(4, null, null),
                new Quiz(5, null, null),
            };

            this.availableExams = new List<Exam>
            {
                new Exam(1, null),
                new Exam(2, null),
                new Exam(3, null),
            };

            this.UnassignedQuizzes = new ObservableCollection<Quiz>();
            this.SelectedExam = new ObservableCollection<Exam>();

            this.QuizUnassignedList.ItemsSource = this.UnassignedQuizzes;
            this.SelectedExamList.ItemsSource = this.SelectedExam;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var subject = this.SubjectTextBox.Text.Trim();

            if (string.IsNullOrEmpty(subject))
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Please enter a subject.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };
                _ = dialog.ShowAsync();
                return;
            }

            if (!this.SelectedExam.Any())
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Please select a final exam for the section.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };
                _ = dialog.ShowAsync();
                return;
            }

            this.SectionCreated?.Invoke(this, new SectionCreatedEventArgs
            {
                Subject = subject,
                AssignedQuizzes = new List<Quiz>(this.UnassignedQuizzes),
                FinalExam = this.SelectedExam.First(),
            });

            this.SubjectTextBox.Text = string.Empty;
            this.UnassignedQuizzes.Clear();
            this.SelectedExam.Clear();

            this.ModalClosed?.Invoke(this, EventArgs.Empty);
        }

        private async void AddExamButton_Click(object sender, RoutedEventArgs e)
        {
            var availableExamsToAdd = this.availableExams
                .Where(exam => !this.SelectedExam.Any(se => se.Id == exam.Id))
                .ToList();

            if (!availableExamsToAdd.Any())
            {
                var noExamsDialog = new ContentDialog
                {
                    Title = "No Exams Available",
                    Content = "All available exams have been assigned to sections.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };
                await noExamsDialog.ShowAsync();
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Select Final Exam",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Add",
                XamlRoot = this.XamlRoot,
            };

            var examListView = new ListView
            {
                SelectionMode = ListViewSelectionMode.Single,
                Height = 300,
                Margin = new Thickness(0, 10, 0, 10),
                ItemTemplate = (DataTemplate)this.Resources["QuizSelectionItemTemplate"],
                ItemsSource = availableExamsToAdd,
            };

            dialog.Content = examListView;

            dialog.PrimaryButtonClick += (s, args) =>
            {
                if (examListView.SelectedItem is Exam selectedExam)
                {
                    this.SelectedExam.Clear();
                    this.SelectedExam.Add(selectedExam);
                }
            };

            await dialog.ShowAsync();
        }

        private async void AddQuizButton_Click(object sender, RoutedEventArgs e)
        {
            var availableQuizzesToAdd = this.availableQuizzes
                .Where(q => !this.UnassignedQuizzes.Any(uq => uq.Id == q.Id))
                .ToList();

            if (!availableQuizzesToAdd.Any())
            {
                var noQuizzesDialog = new ContentDialog
                {
                    Title = "No Quizzes Available",
                    Content = "All available quizzes have been added to the section.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };
                await noQuizzesDialog.ShowAsync();
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Select Quiz",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Add",
                XamlRoot = this.XamlRoot,
            };

            var quizListView = new ListView
            {
                SelectionMode = ListViewSelectionMode.Single,
                Height = 300,
                Margin = new Thickness(0, 10, 0, 10),
                ItemTemplate = (DataTemplate)this.Resources["QuizSelectionItemTemplate"],
                ItemsSource = availableQuizzesToAdd,
            };

            dialog.Content = quizListView;

            dialog.PrimaryButtonClick += (s, args) =>
            {
                if (quizListView.SelectedItem is Quiz selectedQuiz)
                {
                    this.UnassignedQuizzes.Add(selectedQuiz);
                }
            };

            await dialog.ShowAsync();
        }

        private void RemoveQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Quiz quizToRemove)
            {
                this.UnassignedQuizzes.Remove(quizToRemove);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.SubjectTextBox.Text = string.Empty;
            this.UnassignedQuizzes.Clear();
            this.SelectedExam.Clear();

            this.ModalClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Event args for section creation.
    /// </summary>
    public class SectionCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the section subject.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the assigned quizzes.
        /// </summary>
        public List<Quiz> AssignedQuizzes { get; set; } = new List<Quiz>();

        /// <summary>
        /// Gets or sets the final exam.
        /// </summary>
        public Exam FinalExam { get; set; } = null!;
    }
}