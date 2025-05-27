// <copyright file="MultipleChoiceExerciseViewModel.cs" company="YourCompany">
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
    /// ViewModel for handling multiple-choice exercises, including user choices and validation.
    /// </summary>
    public class MultipleChoiceExerciseViewModel : ViewModelBase
    {
        private readonly IExerciseService exerciseService;
        private MultipleChoiceExercise? exercise;
        private ObservableCollection<string>? userChoices;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleChoiceExerciseViewModel"/> class.
        /// </summary>
        /// <param name="service">The exercise service to use for data operations.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="service"/> is null.</exception>
        public MultipleChoiceExerciseViewModel(IExerciseService service)
        {
            try
            {
                this.exerciseService = service ?? throw new ArgumentNullException(nameof(service));
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Initialization Error", $"Failed to initialize MultipleChoiceExerciseViewModel.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets or sets the user's selected choices for the multiple-choice exercise.
        /// </summary>
        public ObservableCollection<string>? UserChoices
        {
            get => this.userChoices;
            set => this.SetProperty(ref this.userChoices, value);
        }

        /// <summary>
        /// Loads the multiple-choice exercise by its ID.
        /// </summary>
        /// <param name="id">The ID of the exercise to load.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetExercise(int id)
        {
            try
            {
                Exercise exercise = await this.exerciseService.GetExerciseById(id);
                if (exercise is MultipleChoiceExercise multipleChoiceExercise)
                {
                    this.exercise = multipleChoiceExercise;
                    this.userChoices = new ObservableCollection<string>();
                }
                else
                {
                    this.RaiseErrorMessage("Exercise Error", $"Invalid exercise type for ID {id}. Expected MultipleChoiceExercise.");
                }
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Exercise Load Error", $"Failed to load exercise with ID {id}.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifies if the user's choices are correct for the current multiple-choice exercise.
        /// </summary>
        /// <returns>True if the choices are correct; otherwise, false.</returns>
        public bool VerifyIfAnswerIsCorrect()
        {
            try
            {
                if (this.exercise == null || this.userChoices == null)
                {
                    this.RaiseErrorMessage("Validation Error", "Exercise or UserChoices is not initialized.");
                    return false;
                }

                return this.exercise.ValidateAnswer(this.userChoices.ToList());
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Validation Error", $"Failed to verify answer.\nDetails: {ex.Message}");
                return false;
            }
        }
    }
}