// <copyright file="ExamPage.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Duo.ViewModels;
    using Duo.ViewModels.Base;
    using Duo.Views.Components;
    using DuoClassLibrary.Models.Exercises;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using static Duo.Views.Components.AssociationExercise;
    using static Duo.Views.Components.FillInTheBlanksExercise;
    using static Duo.Views.Components.MultipleChoiceExercise;

    /// <summary>
    /// A page for displaying and managing an exam.
    /// </summary>
    public sealed partial class ExamPage : Page
    {
        private static readonly SolidColorBrush CorrectBrush = new SolidColorBrush(Microsoft.UI.Colors.Green);
        private static readonly SolidColorBrush IncorrectBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);

        /// <summary>
        /// Initializes a new instance of the <see cref="ExamPage"/> class.
        /// </summary>
        public ExamPage()
        {
            try
            {
                this.InitializeComponent();
                if (this.DataContext is ViewModelBase viewModel)
                {
                    viewModel.ShowErrorMessageRequested += this.ViewModel_ShowErrorMessageRequested;
                }
                else
                {
                    _ = this.ShowErrorMessage("Initialization Error", "DataContext is not set to a valid ViewModel.");
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Initialization Error", $"Failed to initialize ExamPage.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the back button click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.Frame.CanGoBack)
                {
                    this.Frame.GoBack();
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Back Navigation Error", $"Failed to navigate back.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the cancel button click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        public void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.Frame.CanGoBack)
                {
                    this.Frame.GoBack();
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Cancel Navigation Error", $"Failed to cancel and navigate back.\nDetails: {ex.Message}");
            }
        }

        private async void ViewModel_ShowErrorMessageRequested(object? sender, (string Title, string Message) e)
        {
            await this.ShowErrorMessage(e.Title, e.Message);
        }

        private async Task ShowErrorMessage(string title, string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error dialog failed to display. Details: {ex.Message}");
            }
        }

        private void LoadCurrentExercise()
        {
            try
            {
                if (this.ViewModel == null || this.ViewModel.Exercises == null)
                {
                    _ = this.ShowErrorMessage("Load Exercise Error", "ViewModel or Exercises collection is not initialized.");
                    return;
                }

                var currentExercise = this.ViewModel.CurrentExercise;

                if (currentExercise != null)
                {
                    if (currentExercise is DuoClassLibrary.Models.Exercises.AssociationExercise associationExercise)
                    {
                        var associationControl = new Components.AssociationExercise
                        {
                            Question = associationExercise.Question,
                            FirstAnswersList = new ObservableCollection<string>(associationExercise.FirstAnswersList),
                            SecondAnswersList = new ObservableCollection<string>(associationExercise.SecondAnswersList),
                        };
                        associationControl.OnSendClicked += this.AssociationControl_OnSendClicked;
                        this.ExerciseContentControl.Content = associationControl;
                    }
                    else if (currentExercise is FillInTheBlankExercise fillInTheBlanksExercise)
                    {
                        var fillInTheBlanksControl = new Components.FillInTheBlanksExercise
                        {
                            Question = fillInTheBlanksExercise.Question,
                        };
                        fillInTheBlanksControl.OnSendClicked += this.FillInTheBlanksControl_OnSendClicked;
                        this.ExerciseContentControl.Content = fillInTheBlanksControl;
                    }
                    else if (currentExercise is DuoClassLibrary.Models.Exercises.MultipleChoiceExercise multipleChoiceExercise)
                    {
                        var multipleChoiceControl = new Components.MultipleChoiceExercise
                        {
                            Question = multipleChoiceExercise.Question,
                            Answers = new ObservableCollection<MultipleChoiceAnswerModel>(multipleChoiceExercise.Choices),
                        };
                        multipleChoiceControl.OnSendClicked += this.MultipleChoiceControl_OnSendClicked;
                        this.ExerciseContentControl.Content = multipleChoiceControl;
                    }
                    else if (currentExercise is DuoClassLibrary.Models.Exercises.FlashcardExercise flashcardExercise)
                    {
                        var flashcardControl = new Components.FlashcardExercise
                        {
                            Question = flashcardExercise.Question,
                            Answer = flashcardExercise.Answer,
                        };
                        flashcardControl.OnSendClicked += this.FlashcardControl_OnSendClicked;
                        this.ExerciseContentControl.Content = flashcardControl;
                    }
                }
                else
                {
                    this.ExerciseContentControl.Content = null;
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Load Exercise Error", $"Failed to load current exercise.\nDetails: {ex.Message}");
            }
        }

        private void AssociationControl_OnSendClicked(object? sender, AssociationExerciseEventArgs e)
        {
            try
            {
                var contentPairs = e.ContentPairs;
                _ = this.ViewModel.ValidateCurrentExercise(contentPairs);
                var loadedNext = this.ViewModel.LoadNext();

                if (loadedNext)
                {
                    this.LoadCurrentExercise();
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Association Exercise Error", $"Failed to process association exercise response.\nDetails: {ex.Message}");
            }
        }

        private void MultipleChoiceControl_OnSendClicked(object? sender, MultipleChoiceExerciseEventArgs e)
        {
            try
            {
                var contentPairs = e.ContentPairs;
                _ = this.ViewModel.ValidateCurrentExercise(contentPairs);
                var loadedNext = this.ViewModel.LoadNext();

                if (loadedNext)
                {
                    this.LoadCurrentExercise();
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Multiple Choice Exercise Error", $"Failed to process multiple choice exercise response.\nDetails: {ex.Message}");
            }
        }

        private void FillInTheBlanksControl_OnSendClicked(object? sender, FillInTheBlanksExerciseEventArgs e)
        {
            try
            {
                var contentPairs = e.ContentPairs;
                _ = this.ViewModel.ValidateCurrentExercise(contentPairs);
                var loadedNext = this.ViewModel.LoadNext();

                if (loadedNext)
                {
                    this.LoadCurrentExercise();
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Fill In The Blanks Exercise Error", $"Failed to process fill-in-the-blanks exercise response.\nDetails: {ex.Message}");
            }
        }

        private void FlashcardControl_OnSendClicked(object? sender, FlashcardExerciseEventArgs e)
        {
            try
            {
                var contentPairs = e.ContentPairs;
                _ = this.ViewModel.ValidateCurrentExercise(contentPairs);
                var loadedNext = this.ViewModel.LoadNext();

                if (loadedNext)
                {
                    this.LoadCurrentExercise();
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Flashcard Exercise Error", $"Failed to process flashcard exercise response.\nDetails: {ex.Message}");
            }
        }
    }
}