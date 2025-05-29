// <copyright file="FillInTheBlankExercise.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Duo.ViewModels.Base;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using Windows.UI;
    using Windows.UI.ViewManagement;

    /// <summary>
    /// UserControl for fill-in-the-blank exercises.
    /// </summary>
    public sealed partial class FillInTheBlanksExercise : UserControl
    {
        /// <summary>
        /// Occurs when the send button is clicked.
        /// </summary>
        public event EventHandler<FillInTheBlanksExerciseEventArgs>? OnSendClicked;

        /// <summary>
        /// Occurs when the control is clicked.
        /// </summary>
        public event RoutedEventHandler? Click;

        /// <summary>
        /// Identifies the Question dependency property.
        /// </summary>
        public static readonly DependencyProperty QuestionProperty =
            DependencyProperty.Register(nameof(Question), typeof(string), typeof(FillInTheBlanksExercise), new PropertyMetadata(string.Empty));

        private static readonly SolidColorBrush TransparentBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        private static readonly SolidColorBrush SelectedBrush = new SolidColorBrush(Microsoft.UI.Colors.Coral);

        private Button? selectedLeftButton;
        private Button? selectedRightButton;

        /// <summary>
        /// Gets or sets the collection of UI elements representing the question and blanks.
        /// </summary>
        public ObservableCollection<UIElement> QuestionElements { get; set; } = new ObservableCollection<UIElement>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FillInTheBlanksExercise"/> class.
        /// </summary>
        public FillInTheBlanksExercise()
        {
            try
            {
                this.InitializeComponent();
                if (this.DataContext is ViewModelBase viewModel)
                {
                    viewModel.ShowErrorMessageRequested += this.ViewModel_ShowErrorMessageRequested;
                }
                else
                {
                    _ = this.ShowErrorMessage("Initialization Error", "DataContext is not set to a valid ViewModel.");
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Initialization Error", $"Failed to initialize FillInTheBlanksExercise.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        public string Question
        {
            get => (string)this.GetValue(QuestionProperty);
            set
            {
                try
                {
                    this.SetValue(QuestionProperty, value);
                    this.ParseQuestion(value);
                }
                catch (Exception ex)
                {
                    _ = this.ShowErrorMessage("Question Set Error", $"Failed to set question.\nDetails: {ex.Message}");
                }
            }
        }

        private async void ViewModel_ShowErrorMessageRequested(object? sender, (string Title, string Message) e)
        {
            await this.ShowErrorMessage(e.Title, e.Message);
        }

        private async Task ShowErrorMessage(string title, string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error dialog failed to display. Details: {ex.Message}");
            }
        }

        private void ParseQuestion(string question)
        {
            try
            {
                this.QuestionElements.Clear();
                var parts = Regex.Split(question, @"({})");
                var uiSettings = new UISettings();
                SolidColorBrush textColor = new SolidColorBrush(uiSettings.GetColorValue(UIColorType.Foreground));

                foreach (var part in parts)
                {
                    if (part.Contains("{}"))
                    {
                        var textBox = new TextBox
                        {
                            Width = 150,
                            Height = 40,
                            FontSize = 16,
                            PlaceholderText = "Type here...",
                            BorderThickness = new Thickness(1),
                            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)),
                            Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245)),
                            Padding = new Thickness(8, 4, 8, 4),
                            Margin = new Thickness(4),
                            CornerRadius = new CornerRadius(4),
                            SelectionHighlightColor = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)),
                            SelectionHighlightColorWhenNotFocused = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)),
                            VerticalContentAlignment = VerticalAlignment.Center,
                        };

                        textBox.GotFocus += (s, e) =>
                        {
                            try
                            {
                                textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
                                textBox.Background = new SolidColorBrush(Microsoft.UI.Colors.White);
                            }
                            catch (Exception ex)
                            {
                                _ = this.ShowErrorMessage("TextBox Focus Error", $"Failed to update TextBox focus style.\nDetails: {ex.Message}");
                            }
                        };

                        textBox.LostFocus += (s, e) =>
                        {
                            try
                            {
                                textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                                textBox.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                            }
                            catch (Exception ex)
                            {
                                _ = this.ShowErrorMessage("TextBox Focus Error", $"Failed to update TextBox lost focus style.\nDetails: {ex.Message}");
                            }
                        };

                        this.QuestionElements.Add(textBox);
                    }
                    else
                    {
                        var textBlock = new TextBlock
                        {
                            Text = part,
                            FontSize = 16,
                            Foreground = textColor,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(4),
                        };
                        this.QuestionElements.Add(textBlock);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Parse Question Error", $"Failed to parse question.\nDetails: {ex.Message}");
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> inputValues = this.QuestionElements
                    .OfType<TextBox>()
                    .Select(textBox => textBox.Text)
                    .ToList();

                this.OnSendClicked?.Invoke(this, new FillInTheBlanksExerciseEventArgs(inputValues));
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Send Click Error", $"Failed to process send action.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Event args for fill-in-the-blanks send event.
        /// </summary>
        public class FillInTheBlanksExerciseEventArgs : EventArgs
        {
            /// <summary>
            /// Gets the content pairs (user input).
            /// </summary>
            public List<string> ContentPairs { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="FillInTheBlanksExerciseEventArgs"/> class.
            /// </summary>
            /// <param name="inputValues">The input values.</param>
            public FillInTheBlanksExerciseEventArgs(List<string> inputValues)
            {
                this.ContentPairs = inputValues;
            }
        }
    }
}