// <copyright file="MoreDropdown.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A dropdown control for more actions (edit/delete).
    /// </summary>
    public sealed partial class MoreDropdown : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MoreDropdown"/> class.
        /// </summary>
        public MoreDropdown()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Occurs when the edit menu item is clicked.
        /// </summary>
        public event EventHandler<RoutedEventArgs>? EditClicked;

        /// <summary>
        /// Occurs when the delete menu item is clicked.
        /// </summary>
        public event EventHandler<RoutedEventArgs>? DeleteClicked;

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.EditClicked?.Invoke(this, e);
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.DeleteClicked?.Invoke(this, e);
        }
    }
}