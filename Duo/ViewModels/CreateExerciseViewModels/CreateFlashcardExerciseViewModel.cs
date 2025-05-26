// <copyright file="CreateFlashcardExerciseViewModel.cs" company="Duo">
// Copyright (c) Duo. All rights reserved.
// </copyright>

namespace Duo.ViewModels.CreateExerciseViewModels
{
    using System;
    using Duo.ViewModels.Base;
    using Duo.ViewModels.ExerciseViewModels;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Models.Exercises;

    /// <summary>
    /// ViewModel for creating flashcard exercises.
    /// </summary>
    internal partial class CreateFlashcardExerciseViewModel : CreateExerciseViewModelBase
    {
        private string answer = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFlashcardExerciseViewModel"/> class.
        /// </summary>
        public CreateFlashcardExerciseViewModel()
        {
            try
            {
                // No additional initialization required
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage(
                    "Initialization Error",
                    $"Failed to initialize CreateFlashcardExerciseViewModel.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets or sets the answer text for the flashcard exercise.
        /// </summary>
        public string Answer
        {
            get => this.answer;
            set
            {
                try
                {
                    if (this.answer != value)
                    {
                        this.answer = value;
                        this.OnPropertyChanged(nameof(this.Answer));
                    }
                }
                catch (Exception ex)
                {
                    this.RaiseErrorMessage(
                        "Answer Error",
                        $"Failed to set answer.\nDetails: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="FlashcardExercise"/> instance using the specified question, answer, and difficulty.
        /// </summary>
        /// <param name="question">The question text for the flashcard.</param>
        /// <param name="difficulty">The difficulty level of the flashcard.</param>
        /// <returns>A new instance of <see cref="FlashcardExercise"/> or null if creation fails.</returns>
        public override Exercise CreateExercise(string question, Difficulty difficulty)
        {
            try
            {
                return new FlashcardExercise(0, question, this.Answer, difficulty);
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage(
                    "Create Exercise Error",
                    $"Failed to create flashcard exercise.\nDetails: {ex.Message}");
                return null;
            }
        }
    }
}
