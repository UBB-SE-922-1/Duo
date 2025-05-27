namespace Duo.ViewModels.CreateExerciseViewModels
{
    using Duo.ViewModels.ExerciseViewModels;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    /// <summary>
    /// A template selector for different types of exercise view models.
    /// </summary>
    public class ExerciseTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the template for association exercises.
        /// </summary>
        public DataTemplate AssociationExerciseTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for fill-in-the-blank exercises.
        /// </summary>
        public DataTemplate FillInTheBlankExerciseTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for multiple-choice exercises.
        /// </summary>
        public DataTemplate MultipleChoiceExerciseTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for flashcard exercises.
        /// </summary>
        public DataTemplate FlashcardExerciseTemplate { get; set; }

        /// <summary>
        /// Selects the appropriate template based on the type of the item.
        /// </summary>
        /// <param name="item">The item for which to select the template.</param>
        /// <returns>The selected <see cref="DataTemplate"/> or null if no match is found.</returns>
        protected override DataTemplate? SelectTemplateCore(object item)
        {
            if (item is AssociationExerciseViewModel)
            {
                return this.AssociationExerciseTemplate;
            }
            else if (item is FillInTheBlankExerciseViewModel)
            {
                return this.FillInTheBlankExerciseTemplate;
            }
            else if (item is MultipleChoiceExerciseViewModel)
            {
                return this.MultipleChoiceExerciseTemplate;
            }
            else if (item is CreateFlashcardExerciseViewModel)
            {
                return this.FlashcardExerciseTemplate;
            }

            return null;
        }
    }
}
