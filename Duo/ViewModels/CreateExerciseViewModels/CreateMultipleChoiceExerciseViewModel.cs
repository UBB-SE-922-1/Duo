namespace Duo.ViewModels.CreateExerciseViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.ViewModels.Base;
    using Duo.ViewModels.ExerciseViewModels;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Models.Exercises;

    /// <summary>
    /// ViewModel for creating a multiple-choice exercise.
    /// </summary>
    partial class CreateMultipleChoiceExerciseViewModel : CreateExerciseViewModelBase
    {
        private const int MINIMUMANSWERS = 2;
        private const int MAXIMUMANSWERS = 5;

        private readonly ExerciseCreationViewModel parentViewModel;

        private string selectedAnswer = string.Empty;

        /// <summary>
        /// Gets or sets the collection of answers for the multiple-choice exercise.
        /// </summary>
        public ObservableCollection<Answer> Answers { get; set; } = new ObservableCollection<Answer>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateMultipleChoiceExerciseViewModel"/> class.
        /// </summary>
        /// <param name="parentViewModel">The parent view model responsible for exercise creation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parentViewModel"/> is null.</exception>
        public CreateMultipleChoiceExerciseViewModel(ExerciseCreationViewModel parentViewModel)
        {
            try
            {
                this.parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
                this.AddNewAnswerCommand = new RelayCommand(_ => this.AddNewAnswer());
                this.UpdateSelectedAnswerComand = new RelayCommandWithParameter<string>(this.UpdateSelectedAnswer);
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Initialization Error", $"Failed to initialize CreateMultipleChoiceExerciseViewModel.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new multiple-choice exercise.
        /// </summary>
        /// <param name="questionText">The question text for the exercise.</param>
        /// <param name="difficulty">The difficulty level of the exercise.</param>
        /// <returns>A new <see cref="Exercise"/> instance.</returns>
        public override Exercise CreateExercise(string questionText, Difficulty difficulty)
        {
            try
            {
                List<MultipleChoiceAnswerModel> multipleChoiceAnswerModelList = this.GenerateAnswerModelList();
                Exercise newExercise = new MultipleChoiceExercise(0, questionText, difficulty, multipleChoiceAnswerModelList);
                return newExercise;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Create Exercise Error", $"Failed to create multiple choice exercise.\nDetails: {ex.Message}");
                return null; // Fallback, though ideally handled by caller
            }
        }

        /// <summary>
        /// Generates a list of answer models for the multiple-choice exercise.
        /// </summary>
        /// <returns>A list of <see cref="MultipleChoiceAnswerModel"/> instances.</returns>
        public List<MultipleChoiceAnswerModel> GenerateAnswerModelList()
        {
            try
            {
                List<Answer> finalAnswers = this.Answers.ToList();
                List<MultipleChoiceAnswerModel> multipleChoiceAnswerModels = new List<MultipleChoiceAnswerModel>();
                foreach (Answer answer in finalAnswers)
                {
                    multipleChoiceAnswerModels.Add(new MultipleChoiceAnswerModel
                    {
                        AnswerModelId = 0,
                        Answer = answer.Value,
                        IsCorrect = answer.IsCorrect,
                    });
                }

                return multipleChoiceAnswerModels;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Generate Answers Error", $"Failed to generate answer model list.\nDetails: {ex.Message}");
                return new List<MultipleChoiceAnswerModel>();
            }
        }

        /// <summary>
        /// Gets or sets the currently selected answer for the multiple-choice exercise.
        /// </summary>
        public string SelectedAnswer
        {
            get => this.selectedAnswer;
            set
            {
                try
                {
                    this.selectedAnswer = value;
                    this.OnPropertyChanged(nameof(this.SelectedAnswer));
                }
                catch (Exception ex)
                {
                    this.RaiseErrorMessage("Selected Answer Error", $"Failed to set selected answer.\nDetails: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Gets the command to add a new answer to the multiple-choice exercise.
        /// </summary>
        public ICommand AddNewAnswerCommand { get; }

        /// <summary>
        /// Gets the command to update the selected answer.
        /// </summary>
        public ICommand UpdateSelectedAnswerComand { get; }

        private void AddNewAnswer()
        {
            try
            {
                if (this.Answers.Count >= MAXIMUMANSWERS)
                {
                    this.parentViewModel.RaiseErrorMessage("Maximum Answers Reached", $"Maximum number of answers ({MAXIMUMANSWERS}) reached.");
                    return;
                }
                this.Answers.Add(new Answer(string.Empty, false));
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Add Answer Error", $"Failed to add new answer.\nDetails: {ex.Message}");
            }
        }

        private void UpdateSelectedAnswer(string selectedValue)
        {
            try
            {
                foreach (var answer in this.Answers)
                {
                    answer.IsCorrect = answer.Value == selectedValue;
                }

                this.SelectedAnswer = selectedValue; // Update the selected answer reference
                this.OnPropertyChanged(nameof(this.Answers)); // Notify UI
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Update Selected Answer Error", $"Failed to update selected answer.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Represents an answer in the multiple-choice exercise.
        /// </summary>
        public class Answer : ViewModelBase
        {
            private string value;
            private bool isCorrect;

            /// <summary>
            /// Gets or sets the value of the answer (text of the answer).
            /// </summary>
            public string Value
            {
                get => this.value;
                set
                {
                    try
                    {
                        this.value = value;
                        this.OnPropertyChanged(nameof(this.Value));
                    }
                    catch (Exception ex)
                    {
                        this.RaiseErrorMessage("Answer Value Error", $"Failed to set answer value.\nDetails: {ex.Message}");
                    }
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this answer is correct.
            /// </summary>
            public bool IsCorrect
            {
                get => this.isCorrect;
                set
                {
                    try
                    {
                        if (this.isCorrect != value)
                        {
                            this.isCorrect = value;
                            this.OnPropertyChanged(nameof(this.IsCorrect));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.RaiseErrorMessage("Answer IsCorrect Error", $"Failed to set IsCorrect value.\nDetails: {ex.Message}");
                    }
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Answer"/> class.
            /// </summary>
            /// <param name="value">The text of the answer.</param>
            /// <param name="isCorrect">A value indicating whether the answer is correct.</param>
            public Answer(string value, bool isCorrect)
            {
                try
                {
                    this.value = value;
                    this.isCorrect = isCorrect;
                }
                catch (Exception ex)
                {
                    this.RaiseErrorMessage("Answer Initialization Error", $"Failed to initialize Answer.\nDetails: {ex.Message}");
                }
            }
        }
    }
}