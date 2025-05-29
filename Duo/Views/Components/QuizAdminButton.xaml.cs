// <copyright file="QuizAdminButton.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A button control for quiz admin actions.
    /// </summary>
    public sealed partial class QuizAdminButton : UserControl
    {
        /// <summary>
        /// Identifies the Text dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(QuizAdminButton), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Initializes a new instance of the <see cref="QuizAdminButton"/> class.
        /// </summary>
        public QuizAdminButton()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Occurs when the button is clicked.
        /// </summary>
        public event RoutedEventHandler? Click;

        /// <summary>
        /// Gets or sets the button text.
        /// </summary>
        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        private void QuizAdminButton_Click(object sender, RoutedEventArgs e)
        {
            this.Click?.Invoke(this, e);
        }
    }
}