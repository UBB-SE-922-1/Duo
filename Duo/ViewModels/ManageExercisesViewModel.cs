// <copyright file="ManageExercisesViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.ViewModels.Base;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Models.Exercises;
    using DuoClassLibrary.Services;
    using Microsoft.UI.Dispatching;

    /// <summary>
    /// ViewModel for managing exercises in the admin interface.
    /// </summary>
    internal partial class ManageExercisesViewModel : AdminBaseViewModel
    {
        private readonly IExerciseService exerciseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageExercisesViewModel"/> class.
        /// </summary>
        public ManageExercisesViewModel()
        {
            try
            {
                this.exerciseService = (IExerciseService)(App.ServiceProvider?.GetService(typeof(IExerciseService)) ?? throw new InvalidOperationException("ExerciseService not found."));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Service initialization failed", ex.Message);
            }

            this.DeleteExerciseCommand = new RelayCommandWithParameter<Exercise>(exercise => _ = this.DeleteExercise(exercise));
            this.InitializeViewModel();
            _ = this.LoadExercisesAsync();
        }

        /// <summary>
        /// Gets the collection of exercises.
        /// </summary>
        public ObservableCollection<Exercise> Exercises { get; } = new ObservableCollection<Exercise>();

        /// <summary>
        /// Gets the command to delete an exercise.
        /// </summary>
        public ICommand DeleteExerciseCommand { get; }

        /// <summary>
        /// Placeholder for potential future initialization logic.
        /// </summary>
        public void InitializeViewModel()
        {
        }

        /// <summary>
        /// Loads all exercises asynchronously and updates the collection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task LoadExercisesAsync()
        {
            try
            {
                var exercises = await this.exerciseService.GetAllExercises();

                DispatcherQueueHandler callback = () =>
                {
                    this.Exercises.Clear();
                    foreach (var exercise in exercises)
                    {
                        this.Exercises.Add(exercise);
                    }
                };
                _ = DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.Normal, callback: callback);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during LoadExercisesAsync: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                this.RaiseErrorMessage("Failed to load exercises", ex.Message);
            }
        }

        /// <summary>
        /// Deletes an exercise and refreshes the list.
        /// </summary>
        /// <param name="exercise">The exercise to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteExercise(Exercise exercise)
        {
            try
            {
                await this.exerciseService.DeleteExercise(exercise.ExerciseId);
                await this.LoadExercisesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting exercise: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                this.RaiseErrorMessage("Failed to delete exercise", ex.Message);
            }
        }
    }
}