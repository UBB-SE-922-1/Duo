// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateExamViewModel.cs" company="YourCompany">
//   Copyright (c) YourCompany. All rights reserved.
//   Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   ViewModel for creating exams, managing exercises, and interacting with services.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
#pragma warning disable SA1636
namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Models.Exercises;
    using DuoClassLibrary.Models.Quizzes;
    using DuoClassLibrary.Services;

    /// <summary>
    /// ViewModel responsible for creating exams and managing exercise selections.
    /// </summary>
    internal class CreateExamViewModel : AdminBaseViewModel
    {
        private const int MaxExercises = 25;
        private readonly IQuizService quizService;
        private readonly IExerciseService exerciseService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateExamViewModel"/> class.
        /// </summary>
        public CreateExamViewModel()
        {
            try
            {
                this.quizService = (IQuizService?)App.ServiceProvider.GetService(typeof(IQuizService))
                                  ?? throw new InvalidOperationException("QuizService is not registered in the service provider.");
                this.exerciseService = (IExerciseService?)App.ServiceProvider.GetService(typeof(IExerciseService))
                                      ?? throw new InvalidOperationException("ExerciseService is not registered in the service provider.");
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Initialization Error", ex.Message);
                this.quizService = null!;
                this.exerciseService = null!;
            }

            _ = Task.Run(async () => await this.LoadExercisesAsync());

            this.SaveButtonCommand = new RelayCommand(_ => _ = this.CreateExam());
            this.OpenSelectExercisesCommand = new RelayCommand(_ => this.OpenSelectExercises());
            this.RemoveExerciseCommand = new RelayCommandWithParameter<Exercise>(this.RemoveExercise);
        }

        /// <summary>
        /// Occurs when the exercise selection modal should be shown.
        /// </summary>
        public event Action<List<Exercise>> ShowListViewModal;

        /// <summary>
        /// Gets the list of all exercises.
        /// </summary>
        public ObservableCollection<Exercise> Exercises { get; } = new ObservableCollection<Exercise>();

        /// <summary>
        /// Gets the list of selected exercises to be included in the exam.
        /// </summary>
        public ObservableCollection<Exercise> SelectedExercises { get; } = new ObservableCollection<Exercise>();

        /// <summary>
        /// Gets the command used to remove an exercise from the selection.
        /// </summary>
        public ICommand RemoveExerciseCommand { get; }

        /// <summary>
        /// Gets the command used to save the exam.
        /// </summary>
        public ICommand SaveButtonCommand { get; }

        /// <summary>
        /// Gets the command used to open the exercise selection dialog.
        /// </summary>
        public ICommand OpenSelectExercisesCommand { get; }

        /// <summary>
        /// Opens the exercise selection modal.
        /// </summary>
        public void OpenSelectExercises()
        {
            try
            {
                this.ShowListViewModal?.Invoke(this.GetAvailableExercises());
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Open Exercise Selector Error", ex.Message);
            }
        }

        /// <summary>
        /// Gets the exercises that are available for selection (not yet selected).
        /// </summary>
        /// <returns>A list of available exercises.</returns>
        public List<Exercise> GetAvailableExercises()
        {
            try
            {
                var availableExercises = new List<Exercise>();
                foreach (var exercise in this.Exercises)
                {
                    if (!this.SelectedExercises.Contains(exercise))
                    {
                        availableExercises.Add(exercise);
                    }
                }

                return availableExercises;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Get Available Exercises Error", ex.Message);
                return new List<Exercise>();
            }
        }

        /// <summary>
        /// Adds an exercise to the selected list if the maximum is not reached.
        /// </summary>
        /// <param name="selectedExercise">The selected exercise.</param>
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
                    this.RaiseErrorMessage("Add Exercise Error", $"Maximum number of exercises ({MaxExercises}) reached.");
                }
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Add Exercise Error", ex.Message);
            }
        }

        /// <summary>
        /// Removes an exercise from the selected list.
        /// </summary>
        /// <param name="exerciseToBeRemoved">The exercise to remove.</param>
        public void RemoveExercise(Exercise exerciseToBeRemoved)
        {
            try
            {
                this.SelectedExercises.Remove(exerciseToBeRemoved);
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Remove Exercise Error", ex.Message);
            }
        }

        /// <summary>
        /// Asynchronously creates an exam and navigates back.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateExam()
        {
            try
            {
                var newExam = new Exam(0, null);

                foreach (var exercise in this.SelectedExercises)
                {
                    newExam.AddExercise(exercise);
                }

                int examId = await this.quizService.CreateExam(newExam);
                this.GoBack();
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Create Exam Error", ex.Message);
            }
        }

        /// <summary>
        /// Asynchronously loads all available exercises from the service.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
                this.RaiseErrorMessage("Load Exercises Error", ex.Message);
            }
        }
    }
}