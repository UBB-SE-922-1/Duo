// <copyright file="DialogContent.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// Represents the content of a dialog.
    /// </summary>
    public sealed partial class DialogContent : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogContent"/> class.
        /// </summary>
        public DialogContent()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the dialog content text.
        /// </summary>
        public string ContentText { get; set; } = string.Empty;
    }
}