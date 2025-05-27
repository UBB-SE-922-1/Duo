// <copyright file="CreateQuizViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.ViewModels.Base;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Models.Exercises;
    using DuoClassLibrary.Models.Quizzes;
    using DuoClassLibrary.Services;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// ViewModel for creating a quiz and managing its exercises.
    /// </summary>
    internal class CreateQuizViewModel : AdminBaseViewModel
    {
        private const int MaxExercises = 10;

        private readonly IQuizService quizService;
        private readonly IExerciseService exerciseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQuizViewModel"/> class.
        /// </summary>
        public CreateQuizViewModel()
        {
            try
            {
                if (App.ServiceProvider != null)
                {
                    this.quizService = (IQuizService)App.ServiceProvider.GetService(typeof(IQuizService))
                        ?? throw new InvalidOperationException("QuizService not found.");
                    this.exerciseService = (IExerciseService)App.ServiceProvider.GetService(typeof(IExerciseService))
                        ?? throw new InvalidOperationException("ExerciseService not found.");
                }
                else
                {
                    throw new InvalidOperationException("ServiceProvider is not initialized.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Initialization Error", ex.Message);
            }

            _ = Task.Run(async () => await this.LoadExercisesAsync());

            this.SaveButtonCommand = new RelayCommand(_ => _ = this.CreateQuiz());
            this.OpenSelectExercisesCommand = new RelayCommand(_ => this.OpenSelectExercises());
            this.RemoveExerciseCommand = new RelayCommandWithParameter<Exercise>(this.RemoveExercise);
        }

        /// <summary>
        /// Gets the collection of all available exercises.
        /// </summary>
        public ObservableCollection<Exercise> Exercises { get; } = new ObservableCollection<Exercise>();

        /// <summary>
        /// Gets the collection of selected exercises for the quiz.
        /// </summary>
        public ObservableCollection<Exercise> SelectedExercises { get; } = new ObservableCollection<Exercise>();

        /// <summary>
        /// Occurs when the exercise selection modal should be shown.
        /// </summary>
        public event Action<List<Exercise>>? ShowListViewModal;

        /// <summary>
        /// Gets the command to remove an exercise from the quiz.
        /// </summary>
        public ICommand RemoveExerciseCommand { get; }

        /// <summary>
        /// Gets the command to save the quiz.
        /// </summary>
        public ICommand SaveButtonCommand { get; }

        /// <summary>
        /// Gets the command to open the exercise selection dialog.
        /// </summary>
        public ICommand OpenSelectExercisesCommand { get; }

        /// <summary>
        /// Opens the exercise selection dialog.
        /// </summary>
        public void OpenSelectExercises()
        {
            try
            {
                Debug.WriteLine("Opening select exercises...");
                this.ShowListViewModal?.Invoke(this.GetAvailableExercises());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening exercise selection: {ex.Message}");
                this.RaiseErrorMessage("Open Dialog Error", "Could not open the exercise selection dialog.");
            }
        }

        /// <summary>
        /// Gets the list of exercises that are available for selection (not already selected).
        /// </summary>
        /// <returns>A list of available exercises.</returns>
        public List<Exercise> GetAvailableExercises()
        {
            var availableExercises = new List<Exercise>();
            try
            {
                foreach (var exercise in this.Exercises)
                {
                    if (!this.SelectedExercises.Contains(exercise))
                    {
                        availableExercises.Add(exercise);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error filtering available exercises: {ex.Message}");
                this.RaiseErrorMessage("Filter Error", "Could not get available exercises.");
            }

            return availableExercises;
        }

        /// <summary>
        /// Adds an exercise to the selected exercises collection.
        /// </summary>
        /// <param name="selectedExercise">The exercise to add.</param>
        public void AddExercise(Exercise selectedExercise)
        {
            try
            {
                if (this.SelectedExercises.Count < MaxExercises)
                {
                    this.SelectedExercises.Add(selectedExercise);
                }
                else
                {
                    this.RaiseErrorMessage("Limit Reached", $"Maximum number of exercises ({MaxExercises}) reached.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding exercise: {ex.Message}");
                this.RaiseErrorMessage("Add Error", "Could not add the selected exercise.");
            }
        }

        /// <summary>
        /// Removes an exercise from the selected exercises collection.
        /// </summary>
        /// <param name="exerciseToBeRemoved">The exercise to remove.</param>
        public void RemoveExercise(Exercise exerciseToBeRemoved)
        {
            try
            {
                this.SelectedExercises.Remove(exerciseToBeRemoved);
                Debug.WriteLine("Removing exercise...");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing exercise: {ex.Message}");
                this.RaiseErrorMessage("Remove Error", "Could not remove the selected exercise.");
            }
        }

        /// <summary>
        /// Loads all available exercises asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadExercisesAsync()
        {
            try
            {
                this.Exercises.Clear();
                var exercises = await this.exerciseService.GetAllExercises();
                foreach (var exercise in exercises)
                {
                    this.Exercises.Add(exercise);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading exercises: {ex.Message}");
                this.RaiseErrorMessage("Load Error", "Failed to load exercises.");
            }
        }

        /// <summary>
        /// Creates a new quiz with the selected exercises.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CreateQuiz()
        {
            try
            {
                Debug.WriteLine("Creating quiz...");
                var newQuiz = new Quiz(0, null, null);

                foreach (var exercise in this.SelectedExercises)
                {
                    newQuiz.AddExercise(exercise);
                }

                int quizId = await this.quizService.CreateQuiz(newQuiz);

                // await this.quizService.AddExercisesToQuiz(quizId, newQuiz.Exercises);

                this.GoBack();
                Debug.WriteLine(newQuiz);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during CreateQuiz: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                this.RaiseErrorMessage("Quiz Creation Failed", "Something went wrong while creating the quiz.");
            }
        }
    }
}