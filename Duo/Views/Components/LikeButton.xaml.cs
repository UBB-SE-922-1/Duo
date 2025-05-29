// <copyright file="LikeButton.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Input;
    using Microsoft.UI.Xaml.Media.Animation;

    /// <summary>
    /// A button control for liking posts or comments.
    /// </summary>
    public sealed partial class LikeButton : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LikeButton"/> class.
        /// </summary>
        public LikeButton()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Occurs when the like button is clicked.
        /// </summary>
        public event EventHandler<LikeButtonClickedEventArgs>? LikeClicked;

        /// <summary>
        /// Identifies the LikeCount dependency property.
        /// </summary>
        public static readonly DependencyProperty LikeCountProperty =
            DependencyProperty.Register(nameof(LikeCount), typeof(int), typeof(LikeButton), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the like count.
        /// </summary>
        public int LikeCount
        {
            get => (int)this.GetValue(LikeCountProperty);
            set => this.SetValue(LikeCountProperty, value);
        }

        /// <summary>
        /// Identifies the IsLiked dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLikedProperty =
            DependencyProperty.Register(nameof(IsLiked), typeof(bool), typeof(LikeButton), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value indicating whether the item is liked.
        /// </summary>
        public bool IsLiked
        {
            get => (bool)this.GetValue(IsLikedProperty);
            set => this.SetValue(IsLikedProperty, value);
        }

        /// <summary>
        /// Identifies the PostId dependency property.
        /// </summary>
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register(nameof(PostId), typeof(int), typeof(LikeButton), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the post ID.
        /// </summary>
        public int PostId
        {
            get => (int)this.GetValue(PostIdProperty);
            set => this.SetValue(PostIdProperty, value);
        }

        /// <summary>
        /// Identifies the CommentId dependency property.
        /// </summary>
        public static readonly DependencyProperty CommentIdProperty =
            DependencyProperty.Register(nameof(CommentId), typeof(int), typeof(LikeButton), new PropertyMetadata(0));

        /// <summary>
        /// Gets or sets the comment ID.
        /// </summary>
        public int CommentId
        {
            get => (int)this.GetValue(CommentIdProperty);
            set => this.SetValue(CommentIdProperty, value);
        }

        private void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var heartAnimation = this.Resources["HeartAnimation"] as Storyboard;
                heartAnimation?.Begin();

                LikeButtonClickedEventArgs? args = null;

                if (this.PostId > 0)
                {
                    args = new LikeButtonClickedEventArgs(LikeTargetType.Post, this.PostId);
                }
                else if (this.CommentId > 0)
                {
                    args = new LikeButtonClickedEventArgs(LikeTargetType.Comment, this.CommentId);
                }
                else
                {
                    return;
                }

                this.LikeClicked?.Invoke(this, args);
            }
            catch (Exception)
            {
                // Optionally log the exception
            }
        }

        private void LikeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// Increments the like count by one.
        /// </summary>
        public void IncrementLikeCount()
        {
            this.LikeCount++;
        }
    }

    /// <summary>
    /// Enum for like target type.
    /// </summary>
    public enum LikeTargetType
    {
        /// <summary>
        /// The like target is a post.
        /// </summary>
        Post,

        /// <summary>
        /// The like target is a comment.
        /// </summary>
        Comment,
    }

    /// <summary>
    /// Event args for like button click event.
    /// </summary>
    public class LikeButtonClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LikeButtonClickedEventArgs"/> class.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <param name="targetId">The target ID.</param>
        public LikeButtonClickedEventArgs(LikeTargetType targetType, int targetId)
        {
            this.TargetType = targetType;
            this.TargetId = targetId;
        }

        /// <summary>
        /// Gets the target type (post or comment).
        /// </summary>
        public LikeTargetType TargetType { get; }

        /// <summary>
        /// Gets the target ID.
        /// </summary>
        public int TargetId { get; }
    }

}