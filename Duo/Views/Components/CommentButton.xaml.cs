// <copyright file="CommentButton.xaml.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A button control for submitting or opening comments.
    /// </summary>
    public sealed partial class CommentButton : UserControl
    {
        /// <summary>
        /// Occurs when the comment button is clicked.
        /// </summary>
        public event RoutedEventHandler? Click;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentButton"/> class.
        /// </summary>
        public CommentButton()
        {
            this.InitializeComponent();
        }

        private void CommentButton_Click(object sender, RoutedEventArgs e)
        {
            this.Click?.Invoke(this, e);
        }
    }
}