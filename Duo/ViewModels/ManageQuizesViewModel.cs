// <copyright file="ManageQuizesViewModel.cs" company="YourCompany">
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
    /// ViewModel for managing quizzes and their exercises in the admin interface.
    /// </summary>
    internal class ManageQuizesViewModel : AdminBaseViewModel
    {
        private readonly IExerciseService exerciseService;
        private readonly IQuizService quizService;
        private Quiz? selectedQuiz;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageQuizesViewModel"/> class.
        /// </summary>
        public ManageQuizesViewModel()
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

            this.DeleteQuizCommand = new RelayCommandWithParameter<Quiz>(quiz => _ = this.DeleteQuiz(quiz));
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
            this.RemoveExerciseFromQuizCommand = new RelayCommandWithParameter<Exercise>(exercise => _ = this.RemoveExerciseFromQuiz(exercise));
            this.InitView();
        }

        /// <summary>
        /// Gets the collection of quizzes.
        /// </summary>
        public ObservableCollection<Quiz> Quizes { get; } = new ObservableCollection<Quiz>();

        /// <summary>
        /// Gets the collection of exercises in the selected quiz.
        /// </summary>
        public ObservableCollection<Exercise> QuizExercises { get; } = new ObservableCollection<Exercise>();

        /// <summary>
        /// Gets the collection of available exercises.
        /// </summary>
        public ObservableCollection<Exercise> AvailableExercises { get; } = new ObservableCollection<Exercise>();

        /// <summary>
        /// Occurs when the exercise selection modal should be shown.
        /// </summary>
        public event Action<List<Exercise>>? ShowListViewModal;

        /// <summary>
        /// Gets or sets the currently selected quiz.
        /// </summary>
        public Quiz? SelectedQuiz
        {
            get => this.selectedQuiz;
            set
            {
                this.selectedQuiz = value;
                _ = this.UpdateQuizExercises(this.SelectedQuiz);
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the command to delete a quiz.
        /// </summary>
        public ICommand DeleteQuizCommand { get; }

        /// <summary>
        /// Gets the command to open the exercise selection dialog.
        /// </summary>
        public ICommand OpenSelectExercisesCommand { get; }

        /// <summary>
        /// Gets the command to remove an exercise from the selected quiz.
        /// </summary>
        public ICommand RemoveExerciseFromQuizCommand { get; }

        /// <summary>
        /// Initializes the view by loading exercises and quizzes.
        /// </summary>
        public async void InitView()
        {
            await this.LoadExercisesAsync();
            await this.InitializeViewModel();
        }

        /// <summary>
        /// Deletes the specified quiz.
        /// </summary>
        /// <param name="quizToBeDeleted">The quiz to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteQuiz(Quiz quizToBeDeleted)
        {
            try
            {
                Debug.WriteLine("Deleting quiz...");

                if (quizToBeDeleted == this.SelectedQuiz)
                {
                    this.SelectedQuiz = null;
                    await this.UpdateQuizExercises(this.SelectedQuiz);
                }

                foreach (var exercise in quizToBeDeleted.Exercises)
                {
                    this.AvailableExercises.Add(exercise);
                }

                await this.quizService.DeleteQuiz(quizToBeDeleted.Id);
                this.Quizes.Remove(quizToBeDeleted);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during DeleteQuiz: {ex.Message}");
                this.RaiseErrorMessage("Failed to delete quiz", ex.Message);
            }
        }

        /// <summary>
        /// Loads all quizzes into the view model.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeViewModel()
        {
            try
            {
                List<Quiz> quizes = await this.quizService.GetAllQuizzes();
                this.Quizes.Clear();

                foreach (var quiz in quizes)
                {
                    this.Quizes.Add(quiz);
                    Debug.WriteLine(quiz);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to load quizzes", ex.Message);
            }
        }

        /// <summary>
        /// Updates the exercises for the selected quiz.
        /// </summary>
        /// <param name="selectedQuiz">The selected quiz.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateQuizExercises(Quiz? selectedQuiz)
        {
            try
            {
                Debug.WriteLine("Updating quiz exercises...");
                this.QuizExercises.Clear();

                if (selectedQuiz == null)
                {
                    Debug.WriteLine("No quiz selected. Skipping update.");
                    return;
                }

                List<Exercise> exercisesOfSelectedQuiz = await this.exerciseService.GetAllExercisesFromQuiz(selectedQuiz.Id);

                foreach (var exercise in exercisesOfSelectedQuiz)
                {
                    this.QuizExercises.Add(exercise);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during UpdateQuizExercises: {ex.Message}");
                this.RaiseErrorMessage("Failed to load quiz exercises", ex.Message);
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
        /// Adds an exercise to the selected quiz.
        /// </summary>
        /// <param name="selectedExercise">The exercise to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddExercise(Exercise selectedExercise)
        {
            try
            {
                Debug.WriteLine("Adding exercise...");

                if (this.SelectedQuiz == null)
                {
                    this.RaiseErrorMessage("No quiz selected", "Please select a quiz before adding exercises.");
                    return;
                }

                this.SelectedQuiz.AddExercise(selectedExercise);

                await this.quizService.AddExerciseToQuiz(this.SelectedQuiz.Id, selectedExercise.ExerciseId);
                await this.UpdateQuizExercises(this.SelectedQuiz);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while adding exercise: {ex.Message}");
                this.RaiseErrorMessage("Failed to add exercise to quiz", ex.Message);
            }
        }

        /// <summary>
        /// Removes an exercise from the selected quiz.
        /// </summary>
        /// <param name="selectedExercise">The exercise to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RemoveExerciseFromQuiz(Exercise selectedExercise)
        {
            try
            {
                await this.quizService.RemoveExerciseFromQuiz(this.SelectedQuiz.Id, selectedExercise.ExerciseId);
                this.SelectedQuiz.RemoveExercise(selectedExercise);
                await this.UpdateQuizExercises(this.SelectedQuiz);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while removing exercise: {ex.Message}");
                this.RaiseErrorMessage("Failed to remove exercise from quiz", ex.Message);
            }
        }
    }
}