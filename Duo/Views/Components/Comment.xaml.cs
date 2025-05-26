// <copyright file="Comment.xaml.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using Duo.Services;
    using Duo.ViewModels;
    using DuoClassLibrary.Models;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using static Duo.App;

    /// <summary>
    /// Represents a comment control with support for replies, likes, and deletion.
    /// </summary>
    public sealed partial class Comment : UserControl
    {
        /// <summary>
        /// The maximum allowed nesting level for replies.
        /// </summary>
        private const int MaxNestingLevel = 3;

        /// <summary>
        /// Occurs when a reply is submitted.
        /// </summary>
        public event EventHandler<CommentReplyEventArgs>? ReplySubmitted;

        /// <summary>
        /// Occurs when a comment is liked.
        /// </summary>
        public event EventHandler<CommentLikedEventArgs>? CommentLiked;

        /// <summary>
        /// Occurs when a comment is deleted.
        /// </summary>
        public event EventHandler<CommentDeletedEventArgs>? CommentDeleted;

        /// <summary>
        /// Gets the view model for this comment.
        /// </summary>
        public CommentViewModel? ViewModel => this.DataContext as CommentViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="Comment"/> class.
        /// </summary>
        public Comment()
        {
            this.InitializeComponent();

            this.CommentReplyButton.Click += this.CommentReplyButton_Click;
            this.LikeButton.LikeClicked += this.LikeButton_LikeClicked;
            this.ReplyInputControl.CommentSubmitted += this.ReplyInput_CommentSubmitted;
            this.DataContextChanged += this.Comment_DataContextChanged;
            this.ChildCommentsRepeater.ElementPrepared += this.ChildCommentsRepeater_ElementPrepared;
        }

        private void ChildCommentsRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is ContentPresenter presenter)
            {
                var index = args.Index;
                if (this.ViewModel?.Replies != null && index < this.ViewModel.Replies.Count)
                {
                    var childViewModel = this.ViewModel.Replies[index];

                    // Create a Comment control for this child
                    var childComment = new Comment
                    {
                        DataContext = childViewModel,
                        Margin = new Thickness(0, 4, 0, 0),
                    };

                    // Wire up events
                    childComment.ReplySubmitted += this.ChildComment_ReplySubmitted;
                    childComment.CommentLiked += this.ChildComment_CommentLiked;
                    childComment.CommentDeleted += this.ChildComment_CommentDeleted;

                    // Set the content of the presenter
                    presenter.Content = childComment;
                }
            }
        }

        private void Comment_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            // Set up level lines for indentation
            var indentationLevels = new List<int>();
            if (this.ViewModel != null)
            {
                for (int i = 1; i <= this.ViewModel.Level; i++)
                {
                    indentationLevels.Add(i);
                }
            }

            this.LevelLinesRepeater.ItemsSource = indentationLevels;

            // Handle reply button visibility
            this.CommentReplyButton.Visibility = (this.ViewModel != null && this.ViewModel.Level >= MaxNestingLevel)
                ? Visibility.Collapsed
                : Visibility.Visible;

            // Handle toggle button visibility
            this.ToggleChildrenButton.Visibility = (this.ViewModel?.Replies != null && this.ViewModel.Replies.Count > 0)
                ? Visibility.Visible
                : Visibility.Collapsed;

            try
            {
                var currentUser = App.UserService.GetCurrentUser();
                if (currentUser != null && this.ViewModel != null && currentUser.UserId == this.ViewModel.UserId)
                {
                    this.DeleteButton.Visibility = Visibility.Visible;
                }
                else
                {
                    this.DeleteButton.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                this.DeleteButton.Visibility = Visibility.Collapsed;
            }

            // Set initial toggle button state
            if (this.ToggleChildrenButton.Visibility == Visibility.Visible && this.ViewModel != null)
            {
                this.ToggleIcon.Glyph = this.ViewModel.IsExpanded ? "\uE108" : "\uE109";
            }
        }

        /// <summary>
        /// Sets whether the children are collapsed.
        /// </summary>
        /// <param name="collapsed">If set to <c>true</c>, children are collapsed.</param>
        public void SetChildrenCollapsed(bool collapsed)
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.IsExpanded = !collapsed;
            }
        }

        private void ToggleChildrenButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.IsExpanded = !this.ViewModel.IsExpanded;
                this.ToggleIcon.Glyph = this.ViewModel.IsExpanded ? "\uE108" : "\uE109";
            }
        }

        private void LikeButton_LikeClicked(object? sender, LikeButtonClickedEventArgs e)
        {
            if (this.ViewModel != null)
            {
                this.CommentLiked?.Invoke(this, new CommentLikedEventArgs(this.ViewModel.Id));
            }
        }

        private void ChildComment_CommentLiked(object? sender, CommentLikedEventArgs e)
        {
            this.CommentLiked?.Invoke(this, e);
        }

        private void CommentReplyButton_Click(object sender, RoutedEventArgs e)
        {
            this.ShowReplyInput();
        }

        private void ReplyInput_CommentSubmitted(object? sender, EventArgs e)
        {
            if (this.ViewModel == null)
            {
                return;
            }

            if (sender is CommentInput commentInput && !string.IsNullOrWhiteSpace(commentInput.CommentText))
            {
                this.ReplySubmitted?.Invoke(this, new CommentReplyEventArgs(this.ViewModel.Id, commentInput.CommentText));
                commentInput.ClearComment();
                this.HideReplyInput();
            }
        }

        private void ChildComment_ReplySubmitted(object? sender, CommentReplyEventArgs e)
        {
            this.ReplySubmitted?.Invoke(this, e);
        }

        private void ChildComment_CommentDeleted(object? sender, CommentDeletedEventArgs e)
        {
            this.CommentDeleted?.Invoke(this, e);
        }

        private void ShowReplyInput()
        {
            this.ReplyInputControl.Visibility = Visibility.Visible;
            this.ReplyInputControl.Focus(FocusState.Programmatic);
        }

        private void HideReplyInput()
        {
            this.ReplyInputControl.Visibility = Visibility.Collapsed;
            this.ReplyInputControl.ClearComment();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            this.ShowDeleteConfirmation();
        }

        private async void ShowDeleteConfirmation()
        {
            var dialog = new ContentDialog
            {
                Title = "Delete Comment",
                Content = "Are you sure you want to delete this comment? This action cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
            };

            dialog.XamlRoot = this.XamlRoot;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    if (this.ViewModel != null)
                    {
                        int commentId = this.ViewModel.Id;
                        this.CommentDeleted?.Invoke(this, new CommentDeletedEventArgs(commentId));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting comment: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Provides data for the ReplySubmitted event.
    /// </summary>
    public class CommentReplyEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the parent comment ID.
        /// </summary>
        public int ParentCommentId { get; }

        /// <summary>
        /// Gets the reply text.
        /// </summary>
        public string ReplyText { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentReplyEventArgs"/> class.
        /// </summary>
        /// <param name="parentCommentId">The parent comment ID.</param>
        /// <param name="replyText">The reply text.</param>
        public CommentReplyEventArgs(int parentCommentId, string replyText)
        {
            this.ParentCommentId = parentCommentId;
            this.ReplyText = replyText;
        }
    }

    /// <summary>
    /// Provides data for the CommentLiked event.
    /// </summary>
    public class CommentLikedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the comment ID.
        /// </summary>
        public int CommentId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentLikedEventArgs"/> class.
        /// </summary>
        /// <param name="commentId">The comment ID.</param>
        public CommentLikedEventArgs(int commentId)
        {
            this.CommentId = commentId;
        }
    }

    /// <summary>
    /// Provides data for the CommentDeleted event.
    /// </summary>
    public class CommentDeletedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the comment ID.
        /// </summary>
        public int CommentId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentDeletedEventArgs"/> class.
        /// </summary>
        /// <param name="commentId">The comment ID.</param>
        public CommentDeletedEventArgs(int commentId)
        {
            this.CommentId = commentId;
        }
    }
}