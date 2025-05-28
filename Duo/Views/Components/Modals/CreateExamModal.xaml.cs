// <copyright file="CreateExamModal.xaml.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.Views.Components.Modals
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Models.Exercises;
    using DuoClassLibrary.Models.Quizzes;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// Modal dialog for creating an exam.
    /// </summary>
    public sealed partial class CreateExamModal : UserControl
    {
        /// <summary>
        /// Occurs when an exam is created.
        /// </summary>
        public event EventHandler<ExamCreatedEventArgs>? ExamCreated;

        /// <summary>
        /// Occurs when the modal is closed.
        /// </summary>
        public event EventHandler? ModalClosed;

        private const int MaxExercises = 25;
        private const double DefaultPassingThreshold = 90;
        private readonly List<Exercise> availableExercises;

        /// <summary>
        /// Gets the collection of selected exercises.
        /// </summary>
        public ObservableCollection<Exercise> SelectedExercises { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateExamModal"/> class.
        /// </summary>
        public CreateExamModal()
        {
            this.InitializeComponent();

            this.availableExercises = this.CreateSampleExercises();
            this.SelectedExercises = new ObservableCollection<Exercise>();
            this.ExerciseList.ItemsSource = this.SelectedExercises;

            this.SelectedExercises.CollectionChanged += (s, e) => this.UpdateExerciseCount();
            this.PassingThresholdBox.Value = DefaultPassingThreshold;
            this.UpdateExerciseCount();
        }

        private List<Exercise> CreateSampleExercises()
        {
            var exercises = new List<Exercise>();

            try
            {
                exercises.Add(new FillInTheBlankExercise(1, "The capital of France is ___.", Difficulty.Normal, new List<string> { "Paris" }));
                exercises.Add(new FillInTheBlankExercise(2, "The largest planet in our solar system is ___.", Difficulty.Normal, new List<string> { "Jupiter" }));

                var firstList = new List<string> { "H2O", "CO2" };
                var secondList = new List<string> { "Water", "Carbon Dioxide" };
                exercises.Add(new AssociationExercise(3, "Match chemical formulas with their names", Difficulty.Hard, firstList, secondList));

                var choices = new List<MultipleChoiceAnswerModel>
                {
                    new MultipleChoiceAnswerModel("Mercury", true),
                    new MultipleChoiceAnswerModel("Venus", false),
                    new MultipleChoiceAnswerModel("Mars", false),
                };
                exercises.Add(new MultipleChoiceExercise(4, "Which is the closest planet to the Sun?", Difficulty.Normal, choices));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating exercises: {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }

            return exercises;
        }

        private void UpdateExerciseCount()
        {
            this.ExerciseCountText.Text = $"Selected Exercises: {this.SelectedExercises.Count}/{MaxExercises}";
        }

        private async void AddExerciseButton_Click(object sender, RoutedEventArgs e)
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
                ItemsSource = this.availableExercises.Where(ex => !this.SelectedExercises.Contains(ex)).ToList(),
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
                if (this.SelectedExercises.Count < MaxExercises)
                {
                    this.SelectedExercises.Add(selectedExercise);
                }
                else
                {
                    await this.ShowErrorMessage("Cannot add more exercises", $"Maximum number of exercises ({MaxExercises}) reached.");
                }
            }
        }

        private void RemoveExercise_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Exercise exercise)
            {
                this.SelectedExercises.Remove(exercise);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedExercises.Count == 0)
            {
                await this.ShowErrorMessage("No Exercises", "Please add at least one exercise to the exam.");
                return;
            }

            if (this.SelectedExercises.Count < MaxExercises)
            {
                await this.ShowErrorMessage("Insufficient Exercises", $"An exam must have exactly {MaxExercises} exercises.");
                return;
            }

            var exam = new Exam(new Random().Next(1000), null);

            foreach (var exercise in this.SelectedExercises)
            {
                exam.AddExercise(exercise);
            }

            this.ExamCreated?.Invoke(this, new ExamCreatedEventArgs(exam));
            this.ModalClosed?.Invoke(this, EventArgs.Empty);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ModalClosed?.Invoke(this, EventArgs.Empty);
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

    /// <summary>
    /// Event args for exam creation.
    /// </summary>
    public class ExamCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the created exam.
        /// </summary>
        public Exam Exam { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExamCreatedEventArgs"/> class.
        /// </summary>
        /// <param name="exam">The created exam.</param>
        public ExamCreatedEventArgs(Exam exam)
        {
            this.Exam = exam;
        }
    }
}