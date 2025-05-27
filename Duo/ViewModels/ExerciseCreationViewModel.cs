// <copyright file="ExerciseCreationViewModel.cs" company="YourCompany">
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
    using Duo.Helpers.Interfaces;
    using Duo.ViewModels.CreateExerciseViewModels;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Models.Exercises;
    using DuoClassLibrary.Services;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// ViewModel for creating exercises of various types.
    /// </summary>
    internal partial class ExerciseCreationViewModel : AdminBaseViewModel
    {
        private readonly IExerciseService exerciseService;
        private readonly IExerciseViewFactory exerciseViewFactory;
        private object selectedExerciseContent = new TextBlock { Text = "Select an exercise type." };
        private string questionText = string.Empty;
        private string selectedExerciseType = string.Empty;
        private string selectedDifficulty = string.Empty;
        private object currentExerciseViewModel;
        private bool isSuccessMessageVisible;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseCreationViewModel"/> class.
        /// </summary>
        /// <param name="exerciseService">The exercise service.</param>
        /// <param name="exerciseViewFactory">The exercise view factory.</param>
        public ExerciseCreationViewModel(IExerciseService exerciseService, IExerciseViewFactory exerciseViewFactory)
        {
            this.exerciseService = exerciseService ?? throw new ArgumentNullException(nameof(exerciseService));
            this.exerciseViewFactory = exerciseViewFactory ?? throw new ArgumentNullException(nameof(exerciseViewFactory));
            this.CreateMultipleChoiceExerciseViewModel = new CreateMultipleChoiceExerciseViewModel(this);
            this.CreateAssociationExerciseViewModel = new CreateAssociationExerciseViewModel(this);
            this.CreateFillInTheBlankExerciseViewModel = new CreateFillInTheBlankExerciseViewModel(this);
            this.CreateFlashcardExerciseViewModel = new CreateFlashcardExerciseViewModel();
            this.SaveButtonCommand = new RelayCommand(_ => this.CreateExercise());
            this.ExerciseTypes = new ObservableCollection<string>(DuoClassLibrary.Models.Exercises.ExerciseTypes.EXERCISE_TYPES);
            this.Difficulties = new ObservableCollection<string>(DuoClassLibrary.Models.DifficultyList.Difficulties);
            this.SelectedExerciseContent = new TextBlock { Text = "Select an exercise type." };
            this.currentExerciseViewModel = this.CreateAssociationExerciseViewModel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExerciseCreationViewModel"/> class using DI.
        /// </summary>
        public ExerciseCreationViewModel()
        {
            try
            {
                this.exerciseService = (IExerciseService)(App.ServiceProvider.GetService(typeof(IExerciseService)) ?? throw new InvalidOperationException("ExerciseService not found."));
                this.exerciseViewFactory = (IExerciseViewFactory)(App.ServiceProvider.GetService(typeof(IExerciseViewFactory)) ?? throw new InvalidOperationException("ExerciseViewFactory not found."));
                this.CreateMultipleChoiceExerciseViewModel = new CreateMultipleChoiceExerciseViewModel(this);
                this.CreateAssociationExerciseViewModel = new CreateAssociationExerciseViewModel(this);
                this.CreateFillInTheBlankExerciseViewModel = new CreateFillInTheBlankExerciseViewModel(this);
                this.CreateFlashcardExerciseViewModel = new CreateFlashcardExerciseViewModel();
                this.SaveButtonCommand = new RelayCommand(_ => this.CreateExercise());
                this.ExerciseTypes = new ObservableCollection<string>(DuoClassLibrary.Models.Exercises.ExerciseTypes.EXERCISE_TYPES);
                this.Difficulties = new ObservableCollection<string>(DuoClassLibrary.Models.DifficultyList.Difficulties);
                this.SelectedExerciseContent = new TextBlock { Text = "Select an exercise type." };
                this.currentExerciseViewModel = this.CreateAssociationExerciseViewModel;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to initialize ExerciseCreationViewModel", ex.Message);
            }
        }

        /// <summary>
        /// Gets or sets the available exercise types.
        /// </summary>
        public ObservableCollection<string> ExerciseTypes { get; set; }

        /// <summary>
        /// Gets or sets the available difficulties.
        /// </summary>
        public ObservableCollection<string> Difficulties { get; set; }

        /// <summary>
        /// Gets the ViewModel for creating association exercises.
        /// </summary>
        public CreateAssociationExerciseViewModel CreateAssociationExerciseViewModel { get; }

        /// <summary>
        /// Gets the ViewModel for creating fill-in-the-blank exercises.
        /// </summary>
        public CreateFillInTheBlankExerciseViewModel CreateFillInTheBlankExerciseViewModel { get; }

        /// <summary>
        /// Gets the ViewModel for creating multiple-choice exercises.
        /// </summary>
        public CreateMultipleChoiceExerciseViewModel CreateMultipleChoiceExerciseViewModel { get; }

        /// <summary>
        /// Gets the ViewModel for creating flashcard exercises.
        /// </summary>
        public CreateFlashcardExerciseViewModel CreateFlashcardExerciseViewModel { get; } = new();

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        public string QuestionText
        {
            get => this.questionText;
            set
            {
                if (this.questionText != value)
                {
                    this.questionText = value;
                    this.OnPropertyChanged(nameof(this.QuestionText));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected exercise type.
        /// </summary>
        public string SelectedExerciseType
        {
            get => this.selectedExerciseType;
            set
            {
                if (this.selectedExerciseType != value)
                {
                    this.selectedExerciseType = value;
                    this.OnPropertyChanged(nameof(this.SelectedExerciseType));
                    Debug.WriteLine(value);
                    this.UpdateExerciseContent(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected difficulty.
        /// </summary>
        public string SelectedDifficulty
        {
            get => this.selectedDifficulty;
            set
            {
                if (this.selectedDifficulty != value)
                {
                    this.selectedDifficulty = value;
                    this.OnPropertyChanged(nameof(this.SelectedDifficulty));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected exercise content.
        /// </summary>
        public object SelectedExerciseContent
        {
            get => this.selectedExerciseContent;
            set
            {
                if (this.selectedExerciseContent != value)
                {
                    this.selectedExerciseContent = value;
                    this.OnPropertyChanged(nameof(this.SelectedExerciseContent));
                }
            }
        }

        /// <summary>
        /// Gets or sets the current exercise ViewModel.
        /// </summary>
        public object CurrentExerciseViewModel
        {
            get => this.currentExerciseViewModel;
            set
            {
                this.currentExerciseViewModel = value;
                this.OnPropertyChanged(nameof(this.CurrentExerciseViewModel));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the success message is visible.
        /// </summary>
        public bool IsSuccessMessageVisible
        {
            get => this.isSuccessMessageVisible;
            set => this.SetProperty(ref this.isSuccessMessageVisible, value);
        }

        /// <summary>
        /// Gets the command to save the exercise.
        /// </summary>
        public ICommand SaveButtonCommand { get; }

        /// <summary>
        /// Shows the success message for a short duration.
        /// </summary>
        public async void ShowSuccessMessage()
        {
            try
            {
                this.IsSuccessMessageVisible = true;
                await Task.Delay(3000);
                this.IsSuccessMessageVisible = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while showing success message: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the exercise type for testing.
        /// </summary>
        /// <param name="exerciseType">The exercise type.</param>
        public void SetTypeForTest(string exerciseType)
        {
            this.selectedExerciseType = exerciseType;
        }

        /// <summary>
        /// Creates an exercise based on the selected type.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CreateExercise()
        {
            try
            {
                Debug.WriteLine(this.SelectedExerciseType);
                switch (this.SelectedExerciseType)
                {
                    case "Multiple Choice":
                        await this.CreateMultipleChoiceExercise();
                        break;
                    case "Association":
                        await this.CreateAssocitationExercise();
                        break;
                    case "Flashcard":
                        await this.CreateFlashcardExercise();
                        break;
                    case "Fill in the blank":
                        await this.CreateFillInTheBlankExercise();
                        break;
                    default:
                        this.RaiseErrorMessage("Invalid type", "No valid exercise type selected.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Exercise creation failed", ex.Message);
            }
        }

        /// <summary>
        /// Creates a multiple-choice exercise.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CreateMultipleChoiceExercise()
        {
            try
            {
                Difficulty difficulty = this.GetDifficulty(this.SelectedDifficulty);
                Exercise newExercise = this.CreateMultipleChoiceExerciseViewModel.CreateExercise(this.QuestionText, difficulty);
                await this.exerciseService.CreateExercise(newExercise);
                Debug.WriteLine(newExercise);
                this.ShowSuccessMessage();
                this.GoBack();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage(ex.Message, string.Empty);
            }
        }

        /// <summary>
        /// Creates an association exercise.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CreateAssocitationExercise()
        {
            try
            {
                Difficulty difficulty = this.GetDifficulty(this.SelectedDifficulty);
                Exercise newExercise = this.CreateAssociationExerciseViewModel.CreateExercise(this.QuestionText, difficulty);
                await this.exerciseService.CreateExercise(newExercise);
                Debug.WriteLine(newExercise);
                this.ShowSuccessMessage();
                this.GoBack();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage(ex.Message, string.Empty);
            }
        }

        /// <summary>
        /// Creates a flashcard exercise.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CreateFlashcardExercise()
        {
            try
            {
                Difficulty difficulty = this.GetDifficulty(this.SelectedDifficulty);
                Exercise newExercise = this.CreateFlashcardExerciseViewModel.CreateExercise(this.QuestionText, difficulty);
                await this.exerciseService.CreateExercise(newExercise);
                Debug.WriteLine(newExercise);
                this.ShowSuccessMessage();
                this.GoBack();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage(ex.Message, string.Empty);
            }
        }

        /// <summary>
        /// Creates a fill-in-the-blank exercise.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CreateFillInTheBlankExercise()
        {
            try
            {
                Difficulty difficulty = this.GetDifficulty(this.SelectedDifficulty);
                Exercise newExercise = this.CreateFillInTheBlankExerciseViewModel.CreateExercise(this.QuestionText, difficulty);
                await this.exerciseService.CreateExercise(newExercise);
                Debug.WriteLine(newExercise);
                this.ShowSuccessMessage();
                this.GoBack();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage(ex.Message, string.Empty);
            }
        }

        private void UpdateExerciseContent(string exerciseType)
        {
            try
            {
                this.SelectedExerciseContent = this.exerciseViewFactory.CreateExerciseView(exerciseType);

                switch (exerciseType)
                {
                    case "Association":
                        this.CurrentExerciseViewModel = this.CreateAssociationExerciseViewModel;
                        break;
                    case "Fill in the blank":
                        this.CurrentExerciseViewModel = this.CreateFillInTheBlankExerciseViewModel;
                        break;
                    case "Multiple Choice":
                        this.CurrentExerciseViewModel = this.CreateMultipleChoiceExerciseViewModel;
                        break;
                    case "Flashcard":
                        this.CurrentExerciseViewModel = this.CreateFlashcardExerciseViewModel;
                        break;
                    default:
                        this.SelectedExerciseContent = new TextBlock { Text = "Select an exercise type." };
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to update exercise content", ex.Message);
            }
        }

        private Difficulty GetDifficulty(string difficulty)
        {
            try
            {
                return difficulty switch
                {
                    "Easy" => Difficulty.Easy,
                    "Normal" => Difficulty.Normal,
                    "Hard" => Difficulty.Hard,
                    _ => Difficulty.Normal,
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to parse difficulty", ex.Message);
                return Difficulty.Normal;
            }
        }
    }
}