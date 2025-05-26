// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommentCreationViewModel.cs" company="YourCompanyName">
//   Copyright (c) YourCompanyName. All rights reserved.
// </copyright>
// <summary>
//   ViewModel for creating comments.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Duo.ViewModels
{
    using System;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.ViewModels.Base;
    using DuoClassLibrary.Helpers;

    /// <summary>
    /// ViewModel for creating comments.
    /// </summary>
    public class CommentCreationViewModel : ViewModelBase
    {
        private string commentText = string.Empty;
        private string errorMessage = string.Empty;
        private bool isSubmitting;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentCreationViewModel"/> class.
        /// </summary>
        public CommentCreationViewModel()
        {
            this.SubmitCommentCommand = new RelayCommand(
                _ => this.SubmitComment(),
                _ => this.CanSubmitComment());
        }

        /// <summary>
        /// Occurs when a comment is successfully submitted.
        /// </summary>
        public event EventHandler? CommentSubmitted;

        /// <summary>
        /// Gets or sets the text of the comment being created.
        /// </summary>
        public string CommentText
        {
            get => this.commentText;
            set
            {
                if (this.SetProperty(ref this.commentText, value))
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            ValidationHelper.ValidateComment(value);
                            this.ErrorMessage = string.Empty;
                        }
                        else
                        {
                            this.ErrorMessage = "Comment cannot be empty.";
                        }
                    }
                    catch (ArgumentException ex)
                    {
                        this.ErrorMessage = ex.Message;
                    }

                    (this.SubmitCommentCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the error message to display if validation fails.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether a comment is currently being submitted.
        /// </summary>
        public bool IsSubmitting
        {
            get => this.isSubmitting;
            set => this.SetProperty(ref this.isSubmitting, value);
        }

        /// <summary>
        /// Gets the command to submit a comment.
        /// </summary>
        public ICommand SubmitCommentCommand { get; }

        /// <summary>
        /// Clears the comment text and error message.
        /// </summary>
        public void ClearComment()
        {
            this.CommentText = string.Empty;
            this.ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Determines whether the comment can be submitted.
        /// </summary>
        /// <returns>True if the comment can be submitted; otherwise, false.</returns>
        private bool CanSubmitComment()
        {
            return !string.IsNullOrWhiteSpace(this.CommentText) && !this.IsSubmitting && string.IsNullOrEmpty(this.ErrorMessage);
        }

        /// <summary>
        /// Submits the comment if it passes validation.
        /// </summary>
        private void SubmitComment()
        {
            if (!this.CanSubmitComment())
            {
                return;
            }

            this.IsSubmitting = true;
            try
            {
                try
                {
                    ValidationHelper.ValidateComment(this.CommentText);
                }
                catch (ArgumentException ex)
                {
                    this.ErrorMessage = ex.Message;
                    return;
                }

                // Notify subscribers that a comment has been submitted
                this.CommentSubmitted?.Invoke(this, EventArgs.Empty);

                // Clear the comment text
                this.ClearComment();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = $"Error submitting comment: {ex.Message}";
            }
            finally
            {
                this.IsSubmitting = false;
            }
        }
    }
}
