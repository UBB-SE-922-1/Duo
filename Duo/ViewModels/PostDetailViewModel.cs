// <copyright file="PostDetailViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.Services;
    using Duo.ViewModels.Base;
    using Duo.Views.Components;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Services.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using static Duo.App;

    /// <summary>
    /// ViewModel for displaying post details and managing comments.
    /// </summary>
    public class PostDetailViewModel : ViewModelBase
    {
        private readonly IPostService postService;
        private readonly ICommentService commentService;
        private readonly IUserService userService;
        private DuoClassLibrary.Models.Post post;
        private ObservableCollection<CommentViewModel> commentViewModels;
        private ObservableCollection<DuoClassLibrary.Models.Comment> comments;
        private CommentCreationViewModel commentCreationViewModel;
        private bool isLoading;
        private bool hasComments;
        private string errorMessage;
        private object? commentsPanel;
        private string lastProcessedReply = string.Empty;

        /// <summary>
        /// Occurs when comments are loaded.
        /// </summary>
        public event EventHandler? CommentsLoaded;

        /// <summary>
        /// Gets or sets the comments panel object.
        /// </summary>
        public object? CommentsPanel
        {
            get => this.commentsPanel;
            set => this.SetProperty(ref this.commentsPanel, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostDetailViewModel"/> class.
        /// </summary>
        public PostDetailViewModel()
        {
            this.postService = App.PostService;
            this.commentService = App.CommentService;
            this.userService = App.UserService;

            this.post = new DuoClassLibrary.Models.Post
            {
                Title = string.Empty,
                Description = string.Empty,
                Hashtags = new List<string>(),
            };
            this.comments = new ObservableCollection<DuoClassLibrary.Models.Comment>();
            this.commentViewModels = new ObservableCollection<CommentViewModel>();
            this.commentCreationViewModel = new CommentCreationViewModel();
            this.commentCreationViewModel.CommentSubmitted += this.CommentCreationViewModel_CommentSubmitted;

            this.LoadPostDetailsCommand = new RelayCommandWithParameter<int>(this.LoadPostDetails);
            this.AddCommentCommand = new RelayCommandWithParameter<string>(this.AddComment);
            this.AddReplyCommand = new RelayCommandWithParameter<Tuple<int, string>>(this.AddReply);
            this.BackCommand = new RelayCommand(_ => this.GoBack());
            this.errorMessage = string.Empty;
        }

        /// <summary>
        /// Gets or sets the post being displayed.
        /// </summary>
        public DuoClassLibrary.Models.Post Post
        {
            get => this.post;
            set => this.SetProperty(ref this.post, value);
        }

        /// <summary>
        /// Gets or sets the collection of comments.
        /// </summary>
        public ObservableCollection<DuoClassLibrary.Models.Comment> Comments
        {
            get => this.comments;
            set => this.SetProperty(ref this.comments, value);
        }

        /// <summary>
        /// Gets or sets the collection of comment view models.
        /// </summary>
        public ObservableCollection<CommentViewModel> CommentViewModels
        {
            get => this.commentViewModels;
            set => this.SetProperty(ref this.commentViewModels, value);
        }

        /// <summary>
        /// Gets or sets the comment creation view model.
        /// </summary>
        public CommentCreationViewModel CommentCreationViewModel
        {
            get => this.commentCreationViewModel;
            set => this.SetProperty(ref this.commentCreationViewModel, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the view model is loading.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether there are comments.
        /// </summary>
        public bool HasComments
        {
            get => this.hasComments;
            set => this.SetProperty(ref this.hasComments, value);
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            set => this.SetProperty(ref this.errorMessage, value);
        }

        /// <summary>
        /// Gets the command to load post details.
        /// </summary>
        public ICommand LoadPostDetailsCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a comment.
        /// </summary>
        public ICommand AddCommentCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a reply.
        /// </summary>
        public ICommand AddReplyCommand { get; private set; }

        /// <summary>
        /// Gets the command to go back.
        /// </summary>
        public ICommand BackCommand { get; private set; }

        private void GoBack()
        {
            // This is a placeholder - actual navigation will be handled in the view
        }

        private void CommentCreationViewModel_CommentSubmitted(object? sender, EventArgs e)
        {
            if (sender is CommentCreationViewModel viewModel && !string.IsNullOrWhiteSpace(viewModel.CommentText))
            {
                this.AddComment(viewModel.CommentText);
                viewModel.ClearComment();
            }
        }

        /// <summary>
        /// Loads the post details for the specified post ID.
        /// </summary>
        /// <param name="postId">The post ID.</param>
        public async void LoadPostDetails(int postId)
        {
            this.IsLoading = true;
            this.ErrorMessage = string.Empty;

            try
            {
                if (postId <= 0)
                {
                    throw new ArgumentException("Invalid post ID", nameof(postId));
                }

                if (this.Post == null)
                {
                    this.Post = new DuoClassLibrary.Models.Post
                    {
                        Title = string.Empty,
                        Description = string.Empty,
                        Hashtags = new List<string>(),
                    };
                }

                var post = await this.postService.GetPostDetailsWithMetadata(postId);

                if (post != null)
                {
                    this.Post = post;
                    await this.LoadComments(post.Id);
                }
                else
                {
                    this.ErrorMessage = "Post not found";
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = $"Error loading post details: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"LoadPostDetails error: {ex.Message}");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Loads the comments for the specified post ID.
        /// </summary>
        /// <param name="postId">The post ID.</param>
        public async Task LoadComments(int postId)
        {
            try
            {
                if (postId <= 0)
                {
                    throw new ArgumentException("Invalid post ID", nameof(postId));
                }

                var (allComments, topLevelComments, repliesByParentId) = await this.commentService.GetProcessedCommentsByPostId(postId);

                this.Comments.Clear();
                this.CommentViewModels.Clear();

                if (allComments != null && allComments.Any())
                {
                    this.HasComments = true;

                    foreach (var comment in allComments)
                    {
                        this.Comments.Add(comment);
                    }

                    foreach (var comment in topLevelComments)
                    {
                        var commentViewModel = new CommentViewModel(comment, repliesByParentId);
                        this.CommentViewModels.Add(commentViewModel);
                    }

                    this.CommentsLoaded?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    this.HasComments = false;
                }
            }
            catch (Exception ex)
            {
                this.HasComments = false;
                this.ErrorMessage = $"Error loading comments: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"LoadComments error: {ex.Message}");
            }
        }

        private async void AddComment(string commentText)
        {
            try
            {
                await this.commentService.CreateComment(commentText, this.Post.Id, null);
                await this.LoadComments(this.Post.Id);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = $"Error adding comment: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"AddComment error: {ex.Message}");
            }
        }

        private void AddReply(Tuple<int, string> data)
        {
            if (data == null)
            {
                return;
            }

            this.AddReplyToComment(data.Item1, data.Item2);
        }

        /// <summary>
        /// Deletes a comment by its ID.
        /// </summary>
        /// <param name="commentId">The comment ID.</param>
        public async Task DeleteComment(int commentId)
        {
            try
            {
                User currentUser = App.UserService.GetCurrentUser();
                bool success = await this.commentService.DeleteComment(commentId, currentUser.UserId);
                if (success)
                {
                    await this.LoadComments(this.Post.Id);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting comment: {ex.Message}");
            }
        }

        /// <summary>
        /// Likes a comment by its ID.
        /// </summary>
        /// <param name="commentId">The comment ID.</param>
        public async Task LikeCommentById(int commentId)
        {
            try
            {
                bool success = await this.commentService.LikeComment(commentId);
                if (success)
                {
                    await this.LoadComments(this.Post.Id);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error liking comment: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a reply to a comment.
        /// </summary>
        /// <param name="parentCommentId">The parent comment ID.</param>
        /// <param name="replyText">The reply text.</param>
        public async void AddReplyToComment(int parentCommentId, string replyText)
        {
            try
            {
                var result = await this.commentService.CreateReplyWithDuplicateCheck(
                    replyText,
                    this.Post?.Id ?? 0,
                    parentCommentId,
                    this.Comments,
                    this.lastProcessedReply);

                this.lastProcessedReply = result.ReplySignature;
                await this.LoadComments(this.Post.Id);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = $"Error adding reply: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"AddReply error: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds a comment view model by its ID.
        /// </summary>
        /// <param name="commentId">The comment ID.</param>
        /// <returns>The found comment view model, or null if not found.</returns>
        public CommentViewModel FindCommentById(int commentId)
        {
            return this.commentService.FindCommentInHierarchy<CommentViewModel>(
                commentId,
                this.CommentViewModels,
                c => c.Replies,
                c => c.Id);
        }
    }
}