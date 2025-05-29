// <copyright file="CreatePostButton.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A button control for creating a new post.
    /// </summary>
    public sealed partial class CreatePostButton : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePostButton"/> class.
        /// </summary>
        public CreatePostButton()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Occurs when the create post button is clicked.
        /// </summary>
        public event EventHandler<RoutedEventArgs>? CreatePostRequested;

        private void PostButton_Click(object sender, RoutedEventArgs e)
        {
            this.CreatePostRequested?.Invoke(this, e);
        }
    }
}