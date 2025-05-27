// <copyright file="ManageExamsViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using DuoClassLibrary.Models.Exercises;
    using DuoClassLibrary.Models.Quizzes;
    using DuoClassLibrary.Services;

    /// <summary>
    /// ViewModel for managing exams and their exercises in the admin interface.
    /// </summary>
    internal class ManageExamsViewModel : AdminBaseViewModel
    {
        private readonly IExerciseService exerciseService;
        private readonly IQuizService quizService;
        private Exam? selectedExam;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageExamsViewModel"/> class.
        /// </summary>
        public ManageExamsViewModel()
        {
            try
            {
                this.exerciseService = (IExerciseService)(App.ServiceProvider?.GetService(typeof(IExerciseService)) ?? throw new InvalidOperationException("ExerciseService not found."));
                this.quizService = (IQuizService)(App.ServiceProvider?.GetService(typeof(IQuizService)) ?? throw new InvalidOperationException("QuizService not found."));
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Initialization error", ex.Message);
            }

            this.DeleteExamCommand = new RelayCommandWithParameter<Exam>(exam => _ = this.DeleteExam(exam));
            this.OpenSelectExercisesCommand = new RelayCommand(_ =>
            {
                try
                {
                    this.OpenSelectExercises();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"OpenSelectExercises error: {ex.Message}");
                    this.RaiseErrorMessage("Failed to open exercise selection.", ex.Message);
                }
                return Task.CompletedTask;
            });
            this.RemoveExerciseFromQuizCommand = new RelayCommandWithParameter<Exercise>(exercise => _ = this.RemoveExerciseFromExam(exercise));
            this.InitView();
        }

        /// <summary>
        /// Gets the collection of exams.
        /// </summary>
        public ObservableCollection<Exam> Exams { get; } = new ObservableCollection<Exam>();

        /// <summary>
        /// Gets the collection of exercises in the selected exam.
        /// </summary>
        public ObservableCollection<Exercise> ExamExercises { get; } = new ObservableCollection<Exercise>();

        /// <summary>
        /// Gets the collection of available exercises.
        /// </summary>
        public ObservableCollection<Exercise> AvailableExercises { get; } = new ObservableCollection<Exercise>();

        /// <summary>
        /// Occurs when the exercise selection modal should be shown.
        /// </summary>
        public event Action<List<Exercise>>? ShowListViewModal;

        /// <summary>
        /// Gets or sets the currently selected exam.
        /// </summary>
        public Exam? SelectedExam
        {
            get => this.selectedExam;
            set
            {
                this.selectedExam = value;
                _ = this.UpdateExamExercises(this.SelectedExam);
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the command to delete an exam.
        /// </summary>
        public ICommand DeleteExamCommand { get; }

        /// <summary>
        /// Gets the command to open the exercise selection dialog.
        /// </summary>
        public ICommand OpenSelectExercisesCommand { get; }

        /// <summary>
        /// Gets the command to remove an exercise from the selected exam.
        /// </summary>
        public ICommand RemoveExerciseFromQuizCommand { get; }

        /// <summary>
        /// Initializes the view by loading exercises and exams.
        /// </summary>
        public async void InitView()
        {
            await this.LoadExercisesAsync();
            await this.InitializeViewModel();
        }

        /// <summary>
        /// Deletes the specified exam.
        /// </summary>
        /// <param name="examToBeDeleted">The exam to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteExam(Exam examToBeDeleted)
        {
            Debug.WriteLine("Deleting exam...");

            try
            {
                if (examToBeDeleted == this.SelectedExam)
                {
                    this.SelectedExam = null;
                    await this.UpdateExamExercises(this.SelectedExam);
                }

                foreach (var exercise in examToBeDeleted.Exercises)
                {
                    this.AvailableExercises.Add(exercise);
                }

                await this.quizService.DeleteExam(examToBeDeleted.Id);
                this.Exams.Remove(examToBeDeleted);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to delete exam", ex.Message);
            }
        }

        /// <summary>
        /// Loads all exams into the view model.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeViewModel()
        {
            try
            {
                List<Exam> exams = await this.quizService.GetAllExams();
                this.Exams.Clear();

                foreach (var exam in exams)
                {
                    this.Exams.Add(exam);
                    Debug.WriteLine(exam);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to load exams", ex.Message);
            }
        }

        /// <summary>
        /// Updates the exercises for the selected exam.
        /// </summary>
        /// <param name="selectedExam">The selected exam.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateExamExercises(Exam? selectedExam)
        {
            try
            {
                Debug.WriteLine("Updating exam exercises...");
                this.ExamExercises.Clear();

                if (selectedExam == null)
                {
                    Debug.WriteLine("No exam selected. Skipping update.");
                    return;
                }

                List<Exercise> exercises = await this.exerciseService.GetAllExercisesFromExam(selectedExam.Id);
                foreach (var exercise in exercises)
                {
                    this.ExamExercises.Add(exercise);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during UpdateExamExercises: {ex.Message}");
                this.RaiseErrorMessage("Failed to load exam exercises", ex.Message);
            }
        }

        /// <summary>
        /// Opens the exercise selection dialog.
        /// </summary>
        public void OpenSelectExercises()
        {
            try
            {
                Debug.WriteLine("Opening select exercises...");
                this.ShowListViewModal?.Invoke(this.AvailableExercises.ToList());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while opening select exercises: {ex.Message}");
                this.RaiseErrorMessage("Failed to open select exercises", ex.Message);
            }
        }

        /// <summary>
        /// Loads all available exercises into the view model.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadExercisesAsync()
        {
            try
            {
                this.AvailableExercises.Clear();

                var exercises = await this.exerciseService.GetAllExercises();

                foreach (var exercise in exercises)
                {
                    this.AvailableExercises.Add(exercise);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during LoadExercisesAsync: {ex.Message}");
                this.RaiseErrorMessage("Failed to load available exercises", ex.Message);
            }
        }

        /// <summary>
        /// Adds an exercise to the selected exam.
        /// </summary>
        /// <param name="selectedExercise">The exercise to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddExercise(Exercise selectedExercise)
        {
            try
            {
                Debug.WriteLine("Adding exercise...");

                if (this.SelectedExam == null)
                {
                    this.RaiseErrorMessage("No exam selected", "Please select an exam before adding exercises.");
                    return;
                }

                this.SelectedExam.AddExercise(selectedExercise);

                await this.quizService.AddExerciseToExam(this.SelectedExam.Id, selectedExercise.ExerciseId);
                await this.UpdateExamExercises(this.SelectedExam);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while adding exercise: {ex.Message}");
                this.RaiseErrorMessage("Failed to add exercise to exam", ex.Message);
            }
        }

        /// <summary>
        /// Removes an exercise from the selected exam.
        /// </summary>
        /// <param name="selectedExercise">The exercise to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveExerciseFromExam(Exercise selectedExercise)
        {
            try
            {
                Debug.WriteLine("Removing exercise...");

                await this.quizService.RemoveExerciseFromExam(this.SelectedExam.Id, selectedExercise.ExerciseId);
                this.SelectedExam.RemoveExercise(selectedExercise);
                await this.UpdateExamExercises(this.SelectedExam);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while removing exercise: {ex.Message}");
                this.RaiseErrorMessage("Failed to remove exercise from exam", ex.Message);
            }
        }
    }
}