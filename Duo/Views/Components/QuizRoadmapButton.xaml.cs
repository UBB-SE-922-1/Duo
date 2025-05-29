// <copyright file="QuizRoadmapButton.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using System.Diagnostics;
    using System.Windows.Input;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using static Duo.ViewModels.Roadmap.RoadmapButtonTemplate;

    /// <summary>
    /// A button control for displaying a quiz or exam in the roadmap.
    /// </summary>
    public sealed partial class QuizRoadmapButton : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuizRoadmapButton"/> class.
        /// </summary>
        public QuizRoadmapButton()
        {
            this.InitializeComponent();
            this.Loaded += this.QuizRoadmapButton_Loaded;
        }

        /// <summary>
        /// Identifies the QuizId dependency property.
        /// </summary>
        public static readonly DependencyProperty QuizIdProperty =
            DependencyProperty.Register(
                nameof(QuizId),
                typeof(int),
                typeof(QuizRoadmapButton),
                new PropertyMetadata(0, OnQuizIdChanged));

        /// <summary>
        /// Occurs when the button is clicked and no command is set or can execute.
        /// </summary>
        public event RoutedEventHandler? ButtonClick;

        /// <summary>
        /// Identifies the IsExam dependency property.
        /// </summary>
        public static readonly DependencyProperty IsExamProperty =
            DependencyProperty.Register(
                nameof(IsExam),
                typeof(bool),
                typeof(QuizRoadmapButton),
                new PropertyMetadata(false, OnIsExamChanged));

        /// <summary>
        /// Identifies the QuizStatus dependency property.
        /// </summary>
        public static readonly DependencyProperty QuizStatusProperty =
            DependencyProperty.Register(
                nameof(QuizStatus),
                typeof(QUIZ_STATUS),
                typeof(QuizRoadmapButton),
                new PropertyMetadata(QUIZ_STATUS.INCOMPLETE));

        /// <summary>
        /// Gets or sets the quiz or exam ID.
        /// </summary>
        public int QuizId
        {
            get => (int)this.GetValue(QuizIdProperty);
            set => this.SetValue(QuizIdProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this button represents an exam.
        /// </summary>
        public bool IsExam
        {
            get => (bool)this.GetValue(IsExamProperty);
            set => this.SetValue(IsExamProperty, value);
        }

        /// <summary>
        /// Gets or sets the quiz or exam status.
        /// </summary>
        public QUIZ_STATUS QuizStatus
        {
            get => (QUIZ_STATUS)this.GetValue(QuizStatusProperty);
            set => this.SetValue(QuizStatusProperty, value);
        }

        /// <summary>
        /// Identifies the Command dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(QuizRoadmapButton),
                new PropertyMetadata(null));

        /// <summary>
        /// Identifies the CommandParameter dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(QuizRoadmapButton),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the command to execute when the button is clicked.
        /// </summary>
        public ICommand? Command
        {
            get => (ICommand?)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the command parameter.
        /// </summary>
        public object? CommandParameter
        {
            get => this.GetValue(CommandParameterProperty);
            set => this.SetValue(CommandParameterProperty, value);
        }

        private void QuizRoadmapButton_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateQuizId(this.QuizId);
            this.UpdateExamStatus(this.IsExam);
        }

        private static void OnQuizIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is QuizRoadmapButton button)
            {
                int newQuizId = (int)e.NewValue;
                Debug.WriteLine($"Quiz ID changed: {newQuizId}");
                button.UpdateQuizId(newQuizId);
            }
        }

        private static void OnIsExamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is QuizRoadmapButton button)
            {
                bool isExam = (bool)e.NewValue;
                button.UpdateExamStatus(isExam);
            }
        }

        private void UpdateQuizId(int newQuizId)
        {
            if (this.ButtonNumber != null)
            {
                this.ButtonNumber.Text = newQuizId.ToString();
            }

            if (this.ButtonLabel != null)
            {
                this.ButtonLabel.Text = this.IsExam ? $"Exam {newQuizId}" : $"Quiz {newQuizId}";
            }

            if (this.Command != null && this.CircularButton != null)
            {
                this.CircularButton.Tag = this.CommandParameter;
            }
        }

        private void UpdateExamStatus(bool isExam)
        {
            if (this.CircularButton != null)
            {
                if (isExam)
                {
                    switch (this.QuizStatus)
                    {
                        case QUIZ_STATUS.LOCKED:
                            this.CircularButton.Background = new SolidColorBrush(Microsoft.UI.Colors.DarkGray);
                            break;
                        case QUIZ_STATUS.INCOMPLETE:
                            this.CircularButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Olive);
                            break;
                        case QUIZ_STATUS.COMPLETED:
                            this.CircularButton.Background = new SolidColorBrush(Microsoft.UI.Colors.DarkRed);
                            break;
                        default:
                            this.CircularButton.Background = new SolidColorBrush(Microsoft.UI.Colors.DarkGray);
                            break;
                    }
                    this.CircularButton.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
                }
                else
                {
                    switch (this.QuizStatus)
                    {
                        case QUIZ_STATUS.LOCKED:
                            this.CircularButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Gray);
                            break;
                        case QUIZ_STATUS.INCOMPLETE:
                            this.CircularButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Olive);
                            break;
                        case QUIZ_STATUS.COMPLETED:
                            this.CircularButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Green);
                            break;
                        default:
                            this.CircularButton.ClearValue(Button.BackgroundProperty);
                            break;
                    }
                    this.CircularButton.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
                }
            }

            if (this.ButtonLabel != null && this.ButtonNumber != null)
            {
                this.ButtonLabel.Text = isExam ? $"Exam {this.QuizId}" : $"Quiz {this.QuizId}";
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"Button clicked: {this.QuizId}");
            Debug.WriteLine($"Is Exam: {this.IsExam}");
            Debug.WriteLine($"Quiz Status: {this.QuizStatus}");

            if (this.QuizStatus == QUIZ_STATUS.LOCKED)
            {
                return;
            }

            if (this.QuizStatus == QUIZ_STATUS.COMPLETED && this.IsExam)
            {
                return;
            }

            if (this.Command?.CanExecute(this.CommandParameter) == true)
            {
                this.Command.Execute(this.CommandParameter);
            }
            else
            {
                this.ButtonClick?.Invoke(this, e);
            }
        }
    }
}