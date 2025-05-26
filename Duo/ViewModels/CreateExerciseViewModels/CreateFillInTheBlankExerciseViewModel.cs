// <copyright file="CreateFillInTheBlankExerciseViewModel.cs" company="Duo">
// Copyright (c) Duo. All rights reserved.
// </copyright>

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
    /// ViewModel for creating fill-in-the-blank type exercises.
    /// </summary>
    internal partial class CreateFillInTheBlankExerciseViewModel : CreateExerciseViewModelBase
    {
        /// <summary>
        /// Maximum number of answers allowed for this exercise type.
        /// </summary>
        public const int MaxAnswers = 3;

        private readonly ExerciseCreationViewModel parentViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFillInTheBlankExerciseViewModel"/> class.
        /// </summary>
        /// <param name="parentViewModel">The parent exercise creation ViewModel.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parentViewModel"/> is null.</exception>
        public CreateFillInTheBlankExerciseViewModel(ExerciseCreationViewModel parentViewModel)
        {
            try
            {
                this.parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
                this.Answers.Add(new Answer(string.Empty));
                this.AddNewAnswerCommand = new RelayCommand(_ => this.AddNewAnswer());
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage(
                    "Initialization Error",
                    $"Failed to initialize CreateFillInTheBlankExerciseViewModel.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets or sets the collection of answers for the exercise.
        /// </summary>
        public ObservableCollection<Answer> Answers { get; set; } = new ObservableCollection<Answer>();

        /// <summary>
        /// Gets the command to add a new answer.
        /// </summary>
        public ICommand AddNewAnswerCommand { get; private set; }

        /// <summary>
        /// Creates a new <see cref="FillInTheBlankExercise"/> instance based on the provided parameters.
        /// </summary>
        /// <param name="question">The question text for the exercise.</param>
        /// <param name="difficulty">The difficulty level of the exercise.</param>
        /// <returns>A new <see cref="Exercise"/> instance or null if creation fails.</returns>
        public override Exercise CreateExercise(string question, Difficulty difficulty)
        {
            try
            {
                var newExercise = new FillInTheBlankExercise(0, question, difficulty, this.GenerateAnswerList(this.Answers));
                return newExercise;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage(
                    "Create Exercise Error",
                    $"Failed to create fill-in-the-blank exercise.\nDetails: {ex.Message}");
                return new FillInTheBlankExercise(0, string.Empty, Difficulty.Easy, new List<string>());
            }
        }

        /// <summary>
        /// Generates a list of answer strings from an <see cref="ObservableCollection{Answer}"/>.
        /// </summary>
        /// <param name="answers">The collection of <see cref="Answer"/> objects.</param>
        /// <returns>A list of answer strings.</returns>
        public List<string> GenerateAnswerList(ObservableCollection<Answer> answers)
        {
            try
            {
                var answerList = new List<string>();
                foreach (var answer in answers)
                {
                    answerList.Add(answer.Value);
                }

                return answerList;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage(
                    "Generate Answers Error",
                    $"Failed to generate answer list.\nDetails: {ex.Message}");
                return new List<string>();
            }
        }

        /// <summary>
        /// Adds a new empty answer to the collection, if the maximum has not been reached.
        /// </summary>
        public void AddNewAnswer()
        {
            try
            {
                if (this.Answers.Count >= MaxAnswers)
                {
                    this.parentViewModel.RaiseErrorMessage(
                        "Maximum Answers Reached",
                        $"You can only have {MaxAnswers} answers for a fill-in-the-blank exercise.");
                    return;
                }

                this.Answers.Add(new Answer(string.Empty));
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage(
                    "Add Answer Error",
                    $"Failed to add new answer.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Represents a single answer in the fill-in-the-blank exercise.
        /// </summary>
        public class Answer : ViewModelBase
        {
            private string value = string.Empty;

            /// <summary>
            /// Initializes a new instance of the <see cref="Answer"/> class with the specified value.
            /// </summary>
            /// <param name="value">The initial answer value.</param>
            public Answer(string value)
            {
                try
                {
                    this.value = value ?? throw new ArgumentNullException(nameof(value));
                }
                catch (Exception ex)
                {
                    this.RaiseErrorMessage(
                        "Answer Initialization Error",
                        $"Failed to initialize Answer.\nDetails: {ex.Message}");
                }
            }

            /// <summary>
            /// Gets or sets the answer value.
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
                        this.RaiseErrorMessage(
                            "Answer Value Error",
                            $"Failed to set answer value.\nDetails: {ex.Message}");
                    }
                }
            }
        }
    }
}
