// <copyright file="AssociationExerciseViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels.ExerciseViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Duo.ViewModels.Base;
    using DuoClassLibrary.Models.Exercises;
    using DuoClassLibrary.Services;

    /// <summary>
    /// ViewModel for handling association exercises, including user answers and validation.
    /// </summary>
    public class AssociationExerciseViewModel : ViewModelBase
    {
        private readonly IExerciseService exerciseService;
        private AssociationExercise? exercise;
        private ObservableCollection<(string, string)>? userAnswers;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssociationExerciseViewModel"/> class.
        /// </summary>
        /// <param name="exerciseService">The exercise service to use for data operations.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="exerciseService"/> is null.</exception>
        public AssociationExerciseViewModel(IExerciseService exerciseService)
        {
            try
            {
                this.exerciseService = exerciseService ?? throw new ArgumentNullException(nameof(exerciseService));
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Initialization Error", $"Failed to initialize AssociationExerciseViewModel.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets or sets the user's answers for the association exercise.
        /// </summary>
        public ObservableCollection<(string, string)>? UserAnswers
        {
            get => this.userAnswers;
            set => this.SetProperty(ref this.userAnswers, value);
        }

        /// <summary>
        /// Loads the association exercise by its ID.
        /// </summary>
        /// <param name="id">The ID of the exercise to load.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetExercise(int id)
        {
            try
            {
                Exercise exercise = await this.exerciseService.GetExerciseById(id);
                if (exercise is AssociationExercise associationExercise)
                {
                    this.exercise = associationExercise;
                    this.userAnswers = new ObservableCollection<(string, string)>();
                }
                else
                {
                    this.RaiseErrorMessage("Exercise Error", $"Invalid exercise type for ID {id}. Expected AssociationExercise.");
                }
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Exercise Load Error", $"Failed to load exercise with ID {id}.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifies if the user's answers are correct for the current association exercise.
        /// </summary>
        /// <returns>True if the answers are correct; otherwise, false.</returns>
        public bool VerifyIfAnswerIsCorrect()
        {
            try
            {
                if (this.exercise == null || this.userAnswers == null)
                {
                    this.RaiseErrorMessage("Validation Error", "Exercise or UserAnswers is not initialized.");
                    return false;
                }

                return this.exercise.ValidateAnswer(this.userAnswers.ToList());
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Validation Error", $"Failed to verify answer.\nDetails: {ex.Message}");
                return false;
            }
        }
    }
}