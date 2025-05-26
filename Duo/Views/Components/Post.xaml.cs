// <copyright file="Post.xaml.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using CommunityToolkit.WinUI.UI.Controls;
    using Duo.ViewModels;
    using Duo.Views.Pages;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Input;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Navigation;
    using static Duo.App;

    /// <summary>
    /// Represents a post control with like, edit, and delete functionality.
    /// </summary>
    public sealed partial class Post : UserControl
    {
        /// <summary>
        /// The text to display for an unknown user.
        /// </summary>
        private const string UnknownUserText = "Unknown User";

        /// <summary>
        /// Identifies the Username dependency property.
        /// </summary>
        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register(nameof(Username), typeof(string), typeof(Post), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the Date dependency property.
        /// </summary>
        public static readonly DependencyProperty DateProperty =
            DependencyProperty.Register(nameof(Date), typeof(string), typeof(Post), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(Post), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the Content dependency property.
        /// </summary>
        public static new readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(string), typeof(Post), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the LikeCount dependency property.
        /// </summary>
        public static readonly DependencyProperty LikeCountProperty =
            DependencyProperty.Register(nameof(LikeCount), typeof(int), typeof(Post), new PropertyMetadata(0));

        /// <summary>
        /// Identifies the Hashtags dependency property.
        /// </summary>
        public static readonly DependencyProperty HashtagsProperty =
            DependencyProperty.Register(nameof(Hashtags), typeof(IEnumerable<string>), typeof(Post), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the PostId dependency property.
        /// </summary>
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register(nameof(PostId), typeof(int), typeof(Post), new PropertyMetadata(0));

        /// <summary>
        /// Identifies the IsAlwaysHighlighted dependency property.
        /// </summary>
        public static readonly DependencyProperty IsAlwaysHighlightedProperty =
            DependencyProperty.Register(nameof(IsAlwaysHighlighted), typeof(bool), typeof(Post), new PropertyMetadata(false, OnIsAlwaysHighlightedChanged));

        private bool isPointerOver;
        private LikeButton? likeButton;

        /// <summary>
        /// Initializes a new instance of the <see cref="Post"/> class.
        /// </summary>
        public Post()
        {
            this.InitializeComponent();
            this.UpdateMoreOptionsVisibility();
            this.UpdateHighlightState();
            this.Loaded += this.Post_Loaded;
        }

        private static void OnIsAlwaysHighlightedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Post post)
            {
                post.UpdateHighlightState();
            }
        }

        private void Post_Loaded(object sender, RoutedEventArgs e)
        {
            this.likeButton = this.FindDescendant<LikeButton>(this);
            if (this.likeButton != null)
            {
                this.likeButton.LikeClicked += this.LikeButton_LikeClicked;
            }
        }

        private T? FindDescendant<T>(DependencyObject parent)
            where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                {
                    return result;
                }

                T? descendant = this.FindDescendant<T>(child);
                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }

        private async void LikeButton_LikeClicked(object? sender, LikeButtonClickedEventArgs e)
        {
            if (e.TargetType == LikeTargetType.Post && e.TargetId == this.PostId)
            {
                try
                {
                    if (await App.PostService.LikePost(this.PostId))
                    {
                        if (this.likeButton != null)
                        {
                            this.likeButton.IncrementLikeCount();
                        }

                        System.Diagnostics.Debug.WriteLine($"Post liked: ID {this.PostId}, new count: {this.LikeCount}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error liking post: {ex.Message}");
                }
            }
        }

        private void UpdateMoreOptionsVisibility()
        {
            var currentUser = UserService.GetCurrentUser();
            if (currentUser != null)
            {
                this.MoreOptions.Visibility = (this.Username == currentUser.UserName)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            else
            {
                this.MoreOptions.Visibility = Visibility.Collapsed;
            }
        }

        private void PostBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            this.isPointerOver = true;

            if (!this.IsAlwaysHighlighted)
            {
                if (sender is Border border)
                {
                    border.Background = Application.Current.Resources["SystemControlBackgroundAltHighBrush"] as Brush;
                    border.BorderBrush = Application.Current.Resources["SystemControlBackgroundListLowBrush"] as Brush;
                }
            }
        }

        private void PostBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            this.isPointerOver = false;

            if (!this.IsAlwaysHighlighted)
            {
                if (sender is Border border)
                {
                    border.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                    border.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                }
            }
        }

        private void PostBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.IsAlwaysHighlighted)
            {
                return;
            }

            if (this.IsLikeButtonTap(e.OriginalSource as DependencyObject))
            {
                return;
            }

            var frame = this.FindParentFrame();
            if (frame != null)
            {
                var post = new DuoClassLibrary.Models.Post
                {
                    Title = this.Title ?? string.Empty,
                    Description = this.Content ?? string.Empty,
                    Username = this.Username,
                    Date = this.Date,
                    LikeCount = this.LikeCount,
                };

                if (this.Hashtags != null)
                {
                    foreach (var hashtag in this.Hashtags)
                    {
                        post.Hashtags.Add(hashtag);
                    }
                }

                frame.Navigate(typeof(PostDetailPage), post);
            }
        }

        private bool IsLikeButtonTap(DependencyObject? element)
        {
            if (element == null)
            {
                return false;
            }

            if (element is LikeButton)
            {
                return true;
            }

            DependencyObject parent = VisualTreeHelper.GetParent(element);
            while (parent != null && !(parent is Post))
            {
                if (parent is LikeButton)
                {
                    return true;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return false;
        }

        private Frame? FindParentFrame()
        {
            DependencyObject parent = this;
            while (parent != null && !(parent is Frame))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as Frame;
        }

        private async void MoreOptions_EditClicked(object sender, RoutedEventArgs e)
        {
            if (this.Username != $"{UserService.GetCurrentUser().UserName}")
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Error",
                    Content = "You do not have permission to edit this item.",
                    CloseButtonText = "OK",
                };
                await errorDialog.ShowAsync();
                return;
            }

            var dialogComponent = new DialogComponent();

            var post = await PostService.GetPostById(this.PostId);
            if (post == null)
            {
                var errorDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Error",
                    Content = "Post not found in database",
                    CloseButtonText = "OK",
                };
                await errorDialog.ShowAsync();
                return;
            }

            var result = await dialogComponent.ShowEditPostDialog(
                this.XamlRoot,
                this.Title,
                this.Content,
                [.. this.Hashtags],
                post.CategoryID);

            if (result.Success)
            {
                try
                {
                    if (post.CategoryID != result.CommunityId)
                    {
                        ContentDialog errorDialog = new ContentDialog
                        {
                            XamlRoot = this.XamlRoot,
                            Title = "Error",
                            Content = "Changing the post's community/category is not allowed.",
                            CloseButtonText = "OK",
                        };
                        await errorDialog.ShowAsync();
                        return;
                    }

                    post.Title = result.Title;
                    post.Description = result.Content;
                    post.UpdatedAt = DateTime.UtcNow;

                    await PostService.UpdatePost(post);

                    try
                    {
                        var existingHashtags = await PostService.GetHashtagsByPostId(this.PostId);
                        foreach (var hashtag in existingHashtags)
                        {
                            await PostService.RemoveHashtagFromPost(this.PostId, hashtag.Id, UserService.GetCurrentUser().UserId);
                        }

                        foreach (var hashtag in result.Hashtags)
                        {
                            try
                            {
                                var hashtagService = App.HashtagService;
                                var userId = UserService.GetCurrentUser().UserId;

                                var existingHashtag = await hashtagService.GetHashtagByName(hashtag);

                                if (existingHashtag == null)
                                {
                                    var newHashtag = hashtagService.CreateHashtag(hashtag);
                                    await hashtagService.AddHashtagToPost(this.PostId, newHashtag.Id);
                                }
                                else
                                {
                                    await hashtagService.AddHashtagToPost(this.PostId, existingHashtag.Id);
                                }
                            }
                            catch (Exception tagEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error processing hashtag '{hashtag}': {tagEx.Message}");
                            }
                        }

                        this.Hashtags = result.Hashtags;
                    }
                    catch (Exception hashtagEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating hashtags: {hashtagEx.Message}");
                    }

                    this.Title = result.Title;
                    this.Content = result.Content;

                    ContentDialog successDialog = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "Updated",
                        Content = "The post has been successfully updated.",
                        CloseButtonText = "OK",
                    };
                    await successDialog.ShowAsync();
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "Error",
                        Content = "An error occurred while updating the post\n" + ex.Message,
                        CloseButtonText = "OK",
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }

        private async void MoreOptions_DeleteClicked(object sender, RoutedEventArgs e)
        {
            if (this.Username != $"{UserService.GetCurrentUser().UserName}")
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Error",
                    Content = "You do not have permission to delete this item.",
                    CloseButtonText = "OK",
                };
                await errorDialog.ShowAsync();
                return;
            }

            var deleteDialog = new DialogComponent();

            bool isConfirmed = await deleteDialog.ShowConfirmationDialog(
                "Confirm Deletion",
                "Are you sure you want to delete this item?",
                this.XamlRoot);

            if (isConfirmed)
            {
                try
                {
                    await PostService.DeletePost(this.PostId);
                }
                catch (Exception ex)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "Error",
                        Content = "An error occurred while deleting the item. Please try again.\n" + ex.Message,
                        CloseButtonText = "OK",
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                ContentDialog successDialog = new ContentDialog
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Deleted",
                    Content = "The item has been successfully deleted.",
                    CloseButtonText = "OK",
                };
                await successDialog.ShowAsync();

                var frame = this.FindParentFrame();
                if (frame != null)
                {
                    if (frame.Content is PostDetailPage)
                    {
                        if (frame.CanGoBack)
                        {
                            frame.GoBack();
                        }
                    }
                    else if (frame.Content is PostListPage postListPage)
                    {
                        if (postListPage.DataContext is PostListViewModel viewModel)
                        {
                            await viewModel.LoadPosts();
                        }
                    }
                    else if (frame.Content is CategoryPage categoryPage)
                    {
                        categoryPage.RefreshCurrentView();
                    }
                }
            }
        }

        private void MarkdownText_MarkdownRendered(object sender, MarkdownRenderedEventArgs e)
        {
            // This event is fired when the markdown content is rendered
        }

        private async void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri? uri))
            {
                await Windows.System.Launcher.LaunchUriAsync(uri);
            }
        }

        private void UpdateHighlightState()
        {
            if (this.PostBorder != null)
            {
                if (this.IsAlwaysHighlighted)
                {
                    this.PostBorder.Background = Application.Current.Resources["SystemControlBackgroundAltHighBrush"] as Brush;
                    this.PostBorder.BorderBrush = Application.Current.Resources["SystemControlBackgroundListLowBrush"] as Brush;
                }
                else if (!this.isPointerOver)
                {
                    this.PostBorder.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                    this.PostBorder.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                }
            }
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public string Username
        {
            get => (string)this.GetValue(UsernameProperty);
            set
            {
                this.SetValue(UsernameProperty, value);
                this.UpdateMoreOptionsVisibility();
            }
        }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public string Date
        {
            get => (string)this.GetValue(DateProperty);
            set => this.SetValue(DateProperty, value);
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public new string Content
        {
            get => (string)this.GetValue(ContentProperty);
            set => this.SetValue(ContentProperty, value);
        }

        /// <summary>
        /// Gets or sets the like count.
        /// </summary>
        public int LikeCount
        {
            get => (int)this.GetValue(LikeCountProperty);
            set => this.SetValue(LikeCountProperty, value);
        }

        /// <summary>
        /// Gets or sets the hashtags.
        /// </summary>
        public IEnumerable<string> Hashtags
        {
            get => (IEnumerable<string>)this.GetValue(HashtagsProperty);
            set => this.SetValue(HashtagsProperty, value);
        }

        /// <summary>
        /// Gets or sets the post ID.
        /// </summary>
        public int PostId
        {
            get => (int)this.GetValue(PostIdProperty);
            set => this.SetValue(PostIdProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the post is always highlighted.
        /// </summary>
        public bool IsAlwaysHighlighted
        {
            get => (bool)this.GetValue(IsAlwaysHighlightedProperty);
            set => this.SetValue(IsAlwaysHighlightedProperty, value);
        }
    }
}