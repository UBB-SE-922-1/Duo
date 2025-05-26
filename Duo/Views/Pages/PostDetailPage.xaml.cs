// <copyright file="PostDetailPage.xaml.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Duo.Services;
    using Duo.ViewModels;
    using Duo.Views.Components;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Services.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Navigation;
    using static Duo.App;

    /// <summary>
    /// Displays the details of a post and its comments.
    /// </summary>
    public sealed partial class PostDetailPage : Page
    {
        private const int InvalidId = 0;
        private const int DefaultMargin = 16;

        private readonly ICommentService commentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostDetailPage"/> class.
        /// </summary>
        public PostDetailPage()
        {
            this.InitializeComponent();

            this.commentService = App.CommentService;

            this.ViewModel.CommentsPanel = this.CommentsPanel;
            this.ViewModel.CommentsLoaded += this.ViewModel_CommentsLoaded;
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is DuoClassLibrary.Models.Post post && post.Id > InvalidId)
            {
                this.ViewModel.LoadPostDetails(post.Id);
            }
            else
            {
                TextBlock errorText = new TextBlock
                {
                    Text = "Invalid post data received",
                    Foreground = new SolidColorBrush(Colors.Red),
                    Margin = new Thickness(0, 20, 0, 0),
                };
                this.CommentsPanel.Children.Add(errorText);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void CommentInputControl_CommentSubmitted(object? sender, EventArgs e)
        {
            if (sender is CommentInput commentInput && !string.IsNullOrWhiteSpace(commentInput.CommentText))
            {
                if (this.ViewModel.AddCommentCommand.CanExecute(commentInput.CommentText))
                {
                    this.ViewModel.AddCommentCommand.Execute(commentInput.CommentText);
                    commentInput.ClearComment();
                }
            }
        }

        private void ViewModel_CommentsLoaded(object? sender, EventArgs e)
        {
            this.RenderComments();
        }

        private void RenderComments()
        {
            this.CommentsPanel.Children.Clear();

            if (!this.ViewModel.HasComments)
            {
                TextBlock noCommentsText = new TextBlock
                {
                    Text = "No comments yet. Be the first to comment!",
                    Margin = new Thickness(0, DefaultMargin, 0, DefaultMargin),
                };
                this.CommentsPanel.Children.Add(noCommentsText);
                return;
            }

            if (!string.IsNullOrEmpty(this.ViewModel.ErrorMessage))
            {
                TextBlock errorText = new TextBlock
                {
                    Text = this.ViewModel.ErrorMessage,
                    Foreground = new SolidColorBrush(Colors.Red),
                    Margin = new Thickness(0, DefaultMargin, 0, DefaultMargin),
                };
                this.CommentsPanel.Children.Add(errorText);
                return;
            }

            foreach (var commentViewModel in this.ViewModel.CommentViewModels)
            {
                var commentComponent = new Components.Comment
                {
                    DataContext = commentViewModel,
                };

                commentComponent.ReplySubmitted += this.CommentComponent_ReplySubmitted;
                commentComponent.CommentLiked += this.CommentComponent_CommentLiked;
                commentComponent.CommentDeleted += this.CommentComponent_CommentDeleted;

                this.CommentsPanel.Children.Add(commentComponent);
            }
        }

        private void CommentComponent_ReplySubmitted(object? sender, CommentReplyEventArgs e)
        {
            var parameters = new Tuple<int, string>(e.ParentCommentId, e.ReplyText);
            this.ViewModel.AddReplyCommand.Execute(parameters);
        }

        private async void CommentComponent_CommentLiked(object? sender, CommentLikedEventArgs e)
        {
            await this.ViewModel.LikeCommentById(e.CommentId);
        }

        private async void CommentComponent_CommentDeleted(object? sender, CommentDeletedEventArgs e)
        {
            await this.ViewModel.DeleteComment(e.CommentId);
            this.RenderComments();
        }
    }
}