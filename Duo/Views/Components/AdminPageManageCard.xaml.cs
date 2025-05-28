// <copyright file="AdminPageManageCard.xaml.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Input;

    /// <summary>
    /// A card control for admin page management actions.
    /// </summary>
    public sealed partial class AdminPageManageCard : UserControl
    {
        /// <summary>
        /// Occurs when the add button is clicked.
        /// </summary>
        public event EventHandler? AddButtonClicked;

        /// <summary>
        /// Occurs when the manage button is clicked.
        /// </summary>
        public event EventHandler? ManageButtonClicked;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminPageManageCard"/> class.
        /// </summary>
        public AdminPageManageCard()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the title of the card.
        /// </summary>
        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Gets or sets the button text.
        /// </summary>
        public string ButtonText
        {
            get => (string)this.GetValue(ButtonTextProperty);
            set => this.SetValue(ButtonTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the icon for the card.
        /// </summary>
        public IconElement Icon
        {
            get => (IconElement)this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        /// <summary>
        /// Identifies the Icon dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(IconElement), typeof(AdminPageManageCard), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(AdminPageManageCard), new PropertyMetadata("Default Title"));

        /// <summary>
        /// Identifies the ButtonText dependency property.
        /// </summary>
        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(AdminPageManageCard), new PropertyMetadata("add"));

        /// <summary>
        /// Handles the add button click event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        public void HandleClick(object sender, TappedRoutedEventArgs e)
        {
            this.AddButtonClicked?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }

        /// <summary>
        /// Handles the card click event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event arguments.</param>
        public void HandleCardClick(object sender, TappedRoutedEventArgs e)
        {
            this.ManageButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}