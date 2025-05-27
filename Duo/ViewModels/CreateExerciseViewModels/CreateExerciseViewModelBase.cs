namespace Duo.ViewModels.ExerciseViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Duo.ViewModels.Base;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Models.Exercises;

    /// <summary>
    /// Serves as the base class for creating exercise view models.
    /// Provides an abstract method to create an exercise with a specified question and difficulty.
    /// </summary>
    internal abstract class CreateExerciseViewModelBase : ViewModelBase
    {
        /// <summary>
        /// Creates an exercise with the specified question and difficulty.
        /// </summary>
        /// <param name="question">The question text for the exercise.</param>
        /// <param name="difficulty">The difficulty level of the exercise.</param>
        /// <returns>An instance of <see cref="Exercise"/> representing the created exercise.</returns>
        public abstract Exercise CreateExercise(string question, Difficulty difficulty);
    }
}
