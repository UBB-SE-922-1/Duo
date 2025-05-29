// <copyright file="CommentInput.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using DuoClassLibrary.Helpers;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A user control for entering and submitting comments.
    /// </summary>
    public sealed partial class CommentInput : UserControl
    {
        private string commentText = string.Empty;
        private string errorMessage = string.Empty;
        private bool hasError = false;

        /// <summary>
        /// Occurs when a comment is submitted.
        /// </summary>
        public event EventHandler? CommentSubmitted;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentInput"/> class.
        /// </summary>
        public CommentInput()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the comment text.
        /// </summary>
        public string CommentText
        {
            get => this.commentText;
            set
            {
                this.commentText = value;
                try
                {
                    ValidationHelper.ValidateComment(value);
                    this.ErrorMessage = string.Empty;
                    this.HasError = false;
                }
                catch (ArgumentException ex)
                {
                    this.ErrorMessage = ex.Message;
                    this.HasError = true;
                }
            }
        }

        /// <summary>
        /// Gets the error message for the comment input.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            private set
            {
                this.errorMessage = value;
                this.Bindings.Update();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the input has an error.
        /// </summary>
        public bool HasError
        {
            get => this.hasError;
            private set
            {
                this.hasError = value;
                this.Bindings.Update();
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidationHelper.ValidateComment(this.CommentText);
                this.CommentSubmitted?.Invoke(this, EventArgs.Empty);
            }
            catch (ArgumentException ex)
            {
                this.ErrorMessage = ex.Message;
                this.HasError = true;
            }
        }

        /// <summary>
        /// Clears the comment input and error state.
        /// </summary>
        public void ClearComment()
        {
            this.CommentText = string.Empty;
            this.ErrorMessage = string.Empty;
            this.HasError = false;
        }

        /// <summary>
        /// Sets focus to the comment text box.
        /// </summary>
        /// <param name="focusState">The focus state.</param>
        public new void Focus(FocusState focusState)
        {
            this.CommentTextBox.Focus(focusState);
        }
    }
}