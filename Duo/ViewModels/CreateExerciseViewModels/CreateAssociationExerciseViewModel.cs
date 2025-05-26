// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateAssociationExerciseViewModel.cs" company="YourCompanyName">
//   Copyright (c) YourCompanyName. All rights reserved.
//   Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   ViewModel for creating association exercises.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
    /// ViewModel for creating association exercises.
    /// </summary>
    internal partial class CreateAssociationExerciseViewModel : CreateExerciseViewModelBase
    {
        /// <summary>
        /// The minimum number of answers required for the exercise.
        /// </summary>
        public const int MINIMUMANSWERS = 2;

        /// <summary>
        /// The maximum number of answers allowed for the exercise.
        /// </summary>
        public const int MAXIMUMANSWERS = 5;
        private readonly ExerciseCreationViewModel parentViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAssociationExerciseViewModel"/> class.
        /// </summary>
        /// <param name="parentViewModel">The parent view model that manages exercise creation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parentViewModel"/> is null.</exception>
        public CreateAssociationExerciseViewModel(ExerciseCreationViewModel parentViewModel)
        {
            try
            {
                this.parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
                this.LeftSideAnswers.Add(new Answer(string.Empty));
                this.RightSideAnswers.Add(new Answer(string.Empty));
                this.AddNewAnswerCommand = new RelayCommand(_ => this.AddNewAnswer());
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Initialization Error", $"Failed to initialize CreateAssociationExerciseViewModel.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets or sets the collection of answers for the left side of the association exercise.
        /// </summary>
        public ObservableCollection<Answer> LeftSideAnswers { get; set; } = new ();

        /// <summary>
        /// Gets or sets the collection of answers for the right side of the association exercise.
        /// </summary>
        public ObservableCollection<Answer> RightSideAnswers { get; set; } = new ();

        public ICommand AddNewAnswerCommand { get; }

        /// <summary>
        /// Creates an association exercise with the specified question and difficulty level.
        /// </summary>
        /// <param name="question">The question for the exercise.</param>
        /// <param name="difficulty">The difficulty level of the exercise.</param>
        /// <returns>
        /// A new <see cref="AssociationExercise"/> instance if the exercise is successfully created; otherwise, <c>null</c>.
        /// </returns>
        public override Exercise CreateExercise(string question, Difficulty difficulty)
        {
            try
            {
                // Validate: No empty pairings and minimum answers
                var leftAnswers = this.GenerateAnswerList(this.LeftSideAnswers);
                var rightAnswers = this.GenerateAnswerList(this.RightSideAnswers);

                // Check for minimum answers
                if (leftAnswers.Count < MINIMUMANSWERS || rightAnswers.Count < MINIMUMANSWERS)
                {
                    this.parentViewModel.RaiseErrorMessage("Not enough answers", $"You must provide at least {MINIMUMANSWERS} answer pairs.");
                    return null;
                }

                // Check for empty values in any pairing
                for (int i = 0; i < leftAnswers.Count; i++)
                {
                    if (string.IsNullOrWhiteSpace(leftAnswers[i]) || string.IsNullOrWhiteSpace(rightAnswers[i]))
                    {
                        this.parentViewModel.RaiseErrorMessage("Empty Pairing", "All answer pairings must be filled in.");
                        return null;
                    }
                }

                Exercise newExercise = new AssociationExercise(0, question, difficulty, leftAnswers, rightAnswers);
                return newExercise;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Create Exercise Error", $"Failed to create association exercise.\nDetails: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Generates a list of answer values from the provided collection of answers.
        /// </summary>
        /// <param name="answers">The collection of answers to process.</param>
        /// <returns>A list of string values representing the answers.</returns>
        public List<string> GenerateAnswerList(ObservableCollection<Answer> answers)
        {
            try
            {
                List<Answer> finalAnswers = answers.ToList();
                List<string> answersList = new List<string>();
                foreach (Answer answer in finalAnswers)
                {
                    answersList.Add(answer.Value);
                }

                return answersList;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Generate Answers Error", $"Failed to generate answer list.\nDetails: {ex.Message}");
                return new List<string>();
            }
        }

        private void AddNewAnswer()
        {
            try
            {
                if (this.LeftSideAnswers.Count >= MAXIMUMANSWERS || this.RightSideAnswers.Count >= MAXIMUMANSWERS)
                {
                    this.parentViewModel.RaiseErrorMessage("Maximum Answers Reached", "You can only have up to 5 answers.");
                    return;
                }

                Debug.WriteLine("New answer added");
                this.LeftSideAnswers.Add(new Answer(string.Empty));
                this.RightSideAnswers.Add(new Answer(string.Empty));
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Add Answer Error", $"Failed to add new answer.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Represents an answer in the association exercise.
        /// </summary>
        public class Answer : ViewModelBase
        {
            private string value;

            /// <summary>
            /// Initializes a new instance of the <see cref="Answer"/> class.
            /// </summary>
            /// <param name="value">The value of the answer.</param>
            public Answer(string value)
            {
                try
                {
                    this.value = value;
                }
                catch (Exception ex)
                {
                    this.RaiseErrorMessage("Answer Initialization Error", $"Failed to initialize Answer.\nDetails: {ex.Message}");
                }
            }

            /// <summary>
            /// Gets or sets the value of the answer.
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
        }
    }
}