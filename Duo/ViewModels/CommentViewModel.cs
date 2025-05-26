// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommentViewModel.cs" company="YourCompanyName">
//   Copyright (c) YourCompanyName. All rights reserved.
// </copyright>
// <summary>
//   ViewModel for managing comments and their replies.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.Services;
    using Duo.ViewModels.Base;
    using DuoClassLibrary.Helpers;
    using DuoClassLibrary.Models;
    using static Duo.App;

    /// <summary>
    /// ViewModel for managing comments and their replies.
    /// </summary>
    public class CommentViewModel : ViewModelBase
    {
        private const int MAXNESTINGLEVEL = 3;
        private DuoClassLibrary.Models.Comment comment;
        private ObservableCollection<CommentViewModel> replies;
        private bool isExpanded = true;
        private string replyText;
        private bool isReplyVisible;
        private int likeCount;
        private bool isDeleteButtonVisible;
        private bool isReplyButtonVisible;
        private bool isToggleButtonVisible;
        private string toggleIconGlyph = "\uE109"; // Plus icon by default

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentViewModel"/> class.
        /// </summary>
        /// <param name="comment">The comment model associated with this ViewModel.</param>
        /// <param name="repliesByParentId">A dictionary mapping parent comment IDs to their replies.</param>
        public CommentViewModel(Comment comment, Dictionary<int, List<Comment>> repliesByParentId)
        {
            this.comment = comment ?? throw new ArgumentNullException(nameof(comment));
            this.replies = new ObservableCollection<CommentViewModel>();
            this.likeCount = comment.LikeCount;
            this.replyText = string.Empty;

            // Load any child comments/replies
            if (repliesByParentId != null && repliesByParentId.TryGetValue(comment.Id, out var childComments))
            {
                foreach (var reply in childComments)
                {
                    this.replies.Add(new CommentViewModel(reply, repliesByParentId));
                }
            }

            this.ToggleRepliesCommand = new RelayCommand(_ => this.ToggleReplies());
            this.ShowReplyFormCommand = new RelayCommand(_ => this.ShowReplyForm());
            this.CancelReplyCommand = new RelayCommand(_ => this.CancelReply());
            this.LikeCommentCommand = new RelayCommand(_ => this.OnLikeComment());
        }

        /// <summary>
        /// Occurs when a reply is submitted for this comment.
        /// The event provides the comment ID and the reply text as a tuple.
        /// </summary>
        public event EventHandler<Tuple<int, string>>? ReplySubmitted;

        /// <summary>
        /// Gets the unique identifier of the comment.
        /// </summary>
        public int Id => this.comment.Id;

        /// <summary>
        /// Gets the user ID of the comment's author.
        /// </summary>
        public int UserId => this.comment.UserId;

        /// <summary>
        /// Gets the parent comment ID, if any.
        /// </summary>
        public int? ParentCommentId => this.comment.ParentCommentId;

        /// <summary>
        /// Gets the content of the comment.
        /// </summary>
        public string Content => this.comment.Content;

        /// <summary>
        /// Gets the username of the comment's author.
        /// </summary>
        public string Username => this.comment.Username;

        /// <summary>
        /// Gets the formatted creation date of the comment.
        /// </summary>
        public string Date => DateTimeHelper.FormatDate(this.comment.CreatedAt);

        /// <summary>
        /// Gets the nesting level of the comment.
        /// </summary>
        public int Level => this.comment.Level;

        /// <summary>
        /// Gets or sets the like count of the comment.
        /// </summary>
        public int LikeCount
        {
            get => this.likeCount;
            set => this.SetProperty(ref this.likeCount, value);
        }

        /// <summary>
        /// Gets or sets the collection of replies to this comment.
        /// </summary>
        public ObservableCollection<CommentViewModel> Replies
        {
            get => this.replies;
            set => this.SetProperty(ref this.replies, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the comment's replies are expanded.
        /// </summary>
        public bool IsExpanded
        {
            get => this.isExpanded;
            set => this.SetProperty(ref this.isExpanded, value);
        }

        /// <summary>
        /// Gets or sets the text of the reply being composed.
        /// </summary>
        public string ReplyText
        {
            get => this.replyText;
            set => this.SetProperty(ref this.replyText, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the reply form is visible.
        /// </summary>
        public bool IsReplyVisible
        {
            get => this.isReplyVisible;
            set => this.SetProperty(ref this.isReplyVisible, value);
        }

        /// <summary>
        /// Increments the like count for this comment and updates the LikeCount property.
        /// </summary>
        public void LikeComment()
        {
            this.comment.IncrementLikeCount();
            this.LikeCount = this.comment.LikeCount;
        }

        /// <summary>
        /// Gets the command to toggle the visibility of the comment's replies.
        /// </summary>
        public ICommand ToggleRepliesCommand { get; }

        /// <summary>
        /// Gets the command to show the reply form for this comment.
        /// </summary>
        public ICommand ShowReplyFormCommand { get; }

        /// <summary>
        /// Gets the command to cancel the reply being composed.
        /// </summary>
        public ICommand CancelReplyCommand { get; }

        /// <summary>
        /// Gets the command to like the comment.
        /// </summary>
        public ICommand LikeCommentCommand { get; }

        /// <summary>
        /// Toggles the visibility of the comment's replies.
        /// </summary>
        private void ToggleReplies()
        {
            this.IsExpanded = !this.IsExpanded;
        }

        /// <summary>
        /// Shows the reply form for this comment.
        /// </summary>
        private void ShowReplyForm()
        {
            this.IsReplyVisible = true;
            this.ReplyText = string.Empty;
        }

        /// <summary>
        /// Cancels the reply being composed.
        /// </summary>
        private void CancelReply()
        {
            this.IsReplyVisible = false;
            this.ReplyText = string.Empty;
        }

        /// <summary>
        /// Submits a reply to this comment.
        /// </summary>
        private void SubmitReply()
        {
            if (!string.IsNullOrWhiteSpace(this.ReplyText))
            {
                this.ReplySubmitted?.Invoke(this, new Tuple<int, string>(this.Id, this.ReplyText));
                this.IsReplyVisible = false;
                this.ReplyText = string.Empty;
            }
        }

        /// <summary>
        /// Increments the like count for this comment.
        /// </summary>
        private void OnLikeComment()
        {
            this.comment.IncrementLikeCount();
        }
    }
}
