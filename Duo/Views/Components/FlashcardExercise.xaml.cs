// <copyright file="FlashcardExercise.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using DuoClassLibrary.Models.Exercises;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Shapes;

    /// <summary>
    /// Flashcard exercise component that displays a question on the front and answer on the back.
    /// </summary>
    public sealed partial class FlashcardExercise : UserControl
    {
        /// <summary>
        /// Raised when the exercise is completed by clicking OK.
        /// </summary>
        public event EventHandler<bool>? ExerciseCompleted;

        /// <summary>
        /// Raised when the exercise is closed by clicking Close.
        /// </summary>
        public event EventHandler? ExerciseClosed;

        /// <summary>
        /// Raised when the send button is clicked.
        /// </summary>
        public event EventHandler<FlashcardExerciseEventArgs>? OnSendClicked;

        private const double OpacityDefault = 0.5;
        private const string BlankPlaceholder = "___";

        private DispatcherTimer? timer;
        private int timerDuration;
        private TimeSpan remainingTime;
        private TimeSpan elapsedTime;
        private DuoClassLibrary.Models.Exercises.FlashcardExercise? exerciseData;

        /// <summary>
        /// Gets the user's answer from the fill-in-the-gap input.
        /// </summary>
        public string UserAnswer => this.FillInGapInput?.Text ?? string.Empty;

        /// <summary>
        /// Identifies the Topic dependency property.
        /// </summary>
        public static readonly DependencyProperty TopicProperty =
            DependencyProperty.Register(nameof(Topic), typeof(string), typeof(FlashcardExercise), new PropertyMetadata(string.Empty, OnTopicChanged));

        /// <summary>
        /// Gets or sets the topic for category display.
        /// </summary>
        public string Topic
        {
            get => (string)this.GetValue(TopicProperty);
            set => this.SetValue(TopicProperty, value);
        }

        /// <summary>
        /// Identifies the Question dependency property.
        /// </summary>
        public static readonly DependencyProperty QuestionProperty =
            DependencyProperty.Register(nameof(Question), typeof(string), typeof(FlashcardExercise), new PropertyMetadata(string.Empty, OnQuestionChanged));

        /// <summary>
        /// Gets or sets the question.
        /// </summary>
        public string Question
        {
            get => (string)this.GetValue(QuestionProperty);
            set => this.SetValue(QuestionProperty, value);
        }

        /// <summary>
        /// Identifies the Answer dependency property.
        /// </summary>
        public static readonly DependencyProperty AnswerProperty =
            DependencyProperty.Register(nameof(Answer), typeof(string), typeof(FlashcardExercise), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the answer.
        /// </summary>
        public string Answer
        {
            get => (string)this.GetValue(AnswerProperty);
            set => this.SetValue(AnswerProperty, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashcardExercise"/> class.
        /// </summary>
        public FlashcardExercise()
        {
            this.InitializeComponent();
            this.Loaded += this.FlashcardExercise_Loaded;
        }

        private static void OnTopicChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FlashcardExercise flashcard && e.NewValue is string topic)
            {
                flashcard.UpdateTopicDisplay(topic);
            }
        }

        private static void OnQuestionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FlashcardExercise flashcard && e.NewValue is string question)
            {
                flashcard.UpdateQuestionParts(question);
            }
        }

        private void FlashcardExercise_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.timer == null)
            {
                this.SetupTimer();
            }

            this.StartTimer();

            if (string.IsNullOrEmpty(this.QuestionPart1?.Text))
            {
                this.UpdateQuestionParts(this.Question);
            }

            if (this.RightAnswerIcon != null)
            {
                this.RightAnswerIcon.Opacity = OpacityDefault;
            }

            if (this.WrongAnswerIcon != null)
            {
                this.WrongAnswerIcon.Opacity = OpacityDefault;
            }
        }

        private void UpdateTopicDisplay(string topic)
        {
            if (this.TopicTitle != null)
            {
                this.TopicTitle.Text = topic;
            }

            if (this.BackTopicTitle != null)
            {
                this.BackTopicTitle.Text = topic;
            }
        }

        private void UpdateQuestionParts(string question)
        {
            if (this.QuestionPart1 == null || this.QuestionPart2 == null || string.IsNullOrEmpty(question))
            {
                return;
            }

            if (this.QuestionDisplay != null)
            {
                this.QuestionDisplay.Text = question;
            }

            if (question.Contains(BlankPlaceholder))
            {
                string[] parts = question.Split(BlankPlaceholder);
                if (parts.Length >= 2)
                {
                    this.QuestionPart1.Text = parts[0].Trim();
                    this.QuestionPart2.Text = parts[1].Trim();
                    return;
                }
            }

            if (!string.IsNullOrEmpty(this.Answer) && question.Contains(this.Answer, StringComparison.OrdinalIgnoreCase))
            {
                int answerIndex = question.IndexOf(this.Answer, StringComparison.OrdinalIgnoreCase);
                this.QuestionPart1.Text = question.Substring(0, answerIndex).Trim();

                int afterAnswerIndex = answerIndex + this.Answer.Length;
                if (afterAnswerIndex < question.Length)
                {
                    this.QuestionPart2.Text = question.Substring(afterAnswerIndex).Trim();
                }
                else
                {
                    this.QuestionPart2.Text = string.Empty;
                }

                return;
            }

            this.QuestionPart1.Text = "Fill in the answer";
            this.QuestionPart2.Text = string.Empty;
        }

        private int GetTimerDurationByDifficulty(DuoClassLibrary.Models.Difficulty difficulty)
        {
            return difficulty switch
            {
                DuoClassLibrary.Models.Difficulty.Easy => 15,
                DuoClassLibrary.Models.Difficulty.Hard => 45,
                _ => 30,
            };
        }

        private void SetupTimer()
        {
            this.timerDuration = this.GetTimerDurationByDifficulty(this.exerciseData?.Difficulty ?? DuoClassLibrary.Models.Difficulty.Normal);
            this.remainingTime = TimeSpan.FromSeconds(this.timerDuration);
            this.elapsedTime = TimeSpan.Zero;

            if (this.TimerText != null)
            {
                this.TimerText.Text = string.Format("00:{0:00}", this.timerDuration);
            }

            this.timer = new DispatcherTimer();
            this.timer.Interval = TimeSpan.FromSeconds(1);
            this.timer.Tick += this.Timer_Tick;
        }

        private void Timer_Tick(object? sender, object e)
        {
            if (this.remainingTime.TotalSeconds > 0)
            {
                this.remainingTime = this.remainingTime.Subtract(TimeSpan.FromSeconds(1));
                this.elapsedTime = this.elapsedTime.Add(TimeSpan.FromSeconds(1));

                if (this.TimerText != null)
                {
                    int seconds = (int)this.remainingTime.TotalSeconds;
                    this.TimerText.Text = string.Format("00:{0:00}", seconds);
                }

                double progress = 1.0 - (this.remainingTime.TotalSeconds / this.timerDuration);
                this.UpdateTimerVisual(progress);

                if (this.remainingTime.TotalSeconds <= 0)
                {
                    this.PerformCardFlip();
                }
            }
            else
            {
                if (this.FrontSide.Visibility == Visibility.Visible)
                {
                    this.PerformCardFlip();
                }
            }
        }

        private void UpdateTimerVisual(double progress)
        {
            try
            {
                SolidColorBrush redBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                SolidColorBrush orangeBrush = new SolidColorBrush(Microsoft.UI.Colors.Orange);
                SolidColorBrush blackBrush = new SolidColorBrush(Microsoft.UI.Colors.Black);

                if (progress > 0.75)
                {
                    if (this.TimerArc != null)
                    {
                        this.TimerArc.Fill = redBrush;
                    }

                    if (this.TimerText != null)
                    {
                        this.TimerText.Foreground = redBrush;
                    }
                }
                else if (progress > 0.5)
                {
                    if (this.TimerArc != null)
                    {
                        this.TimerArc.Fill = orangeBrush;
                    }

                    if (this.TimerText != null)
                    {
                        this.TimerText.Foreground = orangeBrush;
                    }
                }
                else
                {
                    if (this.TimerArc != null)
                    {
                        this.TimerArc.Fill = blackBrush;
                    }

                    if (this.TimerText != null)
                    {
                        this.TimerText.Foreground = blackBrush;
                    }
                }

                double angle = progress * 360;
                double radians = (angle - 90) * Math.PI / 180.0;
                double radius = 15.0;
                double endX = 20 + (radius * Math.Cos(radians));
                double endY = 20 + (radius * Math.Sin(radians));

                if (this.TimerArc != null)
                {
                    var pathGeometry = new PathGeometry();
                    var figure = new PathFigure { StartPoint = new Windows.Foundation.Point(20, 20) };

                    var lineSegment = new LineSegment { Point = new Windows.Foundation.Point(20, 5) };
                    figure.Segments.Add(lineSegment);

                    var arcSegment = new ArcSegment
                    {
                        Point = new Windows.Foundation.Point(endX, endY),
                        Size = new Windows.Foundation.Size(radius, radius),
                        SweepDirection = SweepDirection.Clockwise,
                        IsLargeArc = angle > 180,
                    };
                    figure.Segments.Add(arcSegment);

                    figure.IsClosed = true;
                    pathGeometry.Figures.Add(figure);

                    this.TimerArc.Data = pathGeometry;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating timer: {ex.Message}");
            }
        }

        private void StartTimer()
        {
            if (this.timer != null && !this.timer.IsEnabled)
            {
                this.timer.Start();
            }
        }

        private void StopTimer()
        {
            if (this.timer != null && this.timer.IsEnabled)
            {
                this.timer.Stop();
            }
        }

        private void ResetTimer()
        {
            try
            {
                this.StopTimer();
                this.elapsedTime = TimeSpan.Zero;
                this.remainingTime = TimeSpan.FromSeconds(this.timerDuration);

                if (this.TimerText != null)
                {
                    this.TimerText.Text = string.Format("00:{0:00}", this.timerDuration);
                    this.TimerText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black);
                }

                if (this.TimerArc != null)
                {
                    this.TimerArc.Fill = new SolidColorBrush(Microsoft.UI.Colors.Black);
                }

                this.UpdateTimerVisual(0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error resetting timer: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes the flashcard exercise with the provided data.
        /// </summary>
        /// <param name="exercise">The flashcard exercise data.</param>
        public void Initialize(DuoClassLibrary.Models.Exercises.FlashcardExercise exercise)
        {
            this.exerciseData = exercise;
            this.Question = exercise.Question;
            this.Answer = exercise.Answer;
            this.FrontSide.Visibility = Visibility.Visible;
            this.BackSide.Visibility = Visibility.Collapsed;

            if (this.FillInGapInput != null)
            {
                this.FillInGapInput.Text = string.Empty;
            }

            if (this.RightAnswerIcon != null)
            {
                this.RightAnswerIcon.Opacity = OpacityDefault;
            }

            if (this.WrongAnswerIcon != null)
            {
                this.WrongAnswerIcon.Opacity = OpacityDefault;
            }

            this.ResetTimer();
            this.SetupTimer();
            this.StartTimer();
            this.UpdateDifficultyIndicator(exercise.Difficulty);
        }

        private void PerformCardFlip()
        {
            var contentPairs = this.UserAnswer;
            this.OnSendClicked?.Invoke(this, new FlashcardExerciseEventArgs(contentPairs));

            if (this.exerciseData != null)
            {
                this.exerciseData.ElapsedTime = this.elapsedTime;
            }

            bool isCorrect = this.IsAnswerCorrect();

            if (this.RightAnswerIcon != null)
            {
                this.RightAnswerIcon.Opacity = isCorrect ? 1.0 : 0.3;
            }

            if (this.WrongAnswerIcon != null)
            {
                this.WrongAnswerIcon.Opacity = isCorrect ? 0.3 : 1.0;

                if (!isCorrect)
                {
                    var wrongPath = this.WrongAnswerIcon.Children.OfType<Path>().FirstOrDefault();
                    if (wrongPath != null)
                    {
                        wrongPath.Stroke = new SolidColorBrush(Microsoft.UI.Colors.Red);
                    }

                    var wrongEllipse = this.WrongAnswerIcon.Children.OfType<Ellipse>().FirstOrDefault();
                    if (wrongEllipse != null)
                    {
                        wrongEllipse.Stroke = new SolidColorBrush(Microsoft.UI.Colors.Red);
                    }
                }
            }

            var okButton = this.FindName("OkButton") as Button;
            if (okButton != null)
            {
                okButton.Background = new SolidColorBrush(
                    isCorrect ?
                    Microsoft.UI.ColorHelper.FromArgb(255, 102, 204, 102) :
                    Microsoft.UI.Colors.Red);
            }

            this.FrontSide.Visibility = Visibility.Collapsed;
            this.BackSide.Visibility = Visibility.Visible;
        }

        private void FlipButton_Click(object sender, RoutedEventArgs e)
        {
            this.StopTimer();
            this.PerformCardFlip();
        }

        private bool IsAnswerCorrect()
        {
            if (string.IsNullOrEmpty(this.UserAnswer) || string.IsNullOrEmpty(this.Answer))
            {
                return false;
            }

            return this.UserAnswer.Trim().Equals(this.Answer.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.StopTimer();

            if (this.exerciseData != null)
            {
                this.exerciseData.ElapsedTime = this.elapsedTime;
            }

            this.ExerciseClosed?.Invoke(this, EventArgs.Empty);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.StopTimer();

            if (this.exerciseData != null)
            {
                this.exerciseData.ElapsedTime = this.elapsedTime;
            }

            this.ExerciseCompleted?.Invoke(this, this.IsAnswerCorrect());
        }

        /// <summary>
        /// Resets the flashcard exercise UI and timer.
        /// </summary>
        public void Reset()
        {
            this.FrontSide.Visibility = Visibility.Visible;
            this.BackSide.Visibility = Visibility.Collapsed;

            if (this.FillInGapInput != null)
            {
                this.FillInGapInput.Text = string.Empty;
            }

            if (this.RightAnswerIcon != null)
            {
                this.RightAnswerIcon.Opacity = OpacityDefault;
            }

            if (this.WrongAnswerIcon != null)
            {
                this.WrongAnswerIcon.Opacity = OpacityDefault;
            }

            this.ResetTimer();
            this.StartTimer();
        }

        /// <summary>
        /// Gets the elapsed time for the exercise.
        /// </summary>
        /// <returns>The elapsed time.</returns>
        public TimeSpan GetElapsedTime()
        {
            return this.elapsedTime;
        }

        private void UpdateDifficultyIndicator(DuoClassLibrary.Models.Difficulty difficulty)
        {
            int seconds = this.GetTimerDurationByDifficulty(difficulty);
            this.timerDuration = seconds;
            this.remainingTime = TimeSpan.FromSeconds(seconds);

            if (this.TimerText != null)
            {
                this.TimerText.Text = string.Format("00:{0:00}", seconds);
                this.TimerText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black);
            }

            if (this.TimerArc != null)
            {
                this.TimerArc.Fill = new SolidColorBrush(Microsoft.UI.Colors.Black);
            }

            try
            {
                var difficultyText = this.FindName("DifficultyText") as TextBlock;
                if (difficultyText != null)
                {
                    difficultyText.Text = difficulty.ToString();
                }

                var difficultyBars = this.FindName("DifficultyBars") as StackPanel;
                if (difficultyBars != null && difficultyBars.Children.Count >= 3)
                {
                    var easyBar = difficultyBars.Children[0] as Rectangle;
                    var normalBar = difficultyBars.Children[1] as Rectangle;
                    var hardBar = difficultyBars.Children[2] as Rectangle;

                    if (easyBar != null && normalBar != null && hardBar != null)
                    {
                        easyBar.Opacity = 0.3;
                        normalBar.Opacity = 0.3;
                        hardBar.Opacity = 0.3;

                        switch (difficulty)
                        {
                            case DuoClassLibrary.Models.Difficulty.Easy:
                                easyBar.Opacity = 1.0;
                                break;
                            case DuoClassLibrary.Models.Difficulty.Normal:
                                easyBar.Opacity = 1.0;
                                normalBar.Opacity = 1.0;
                                break;
                            case DuoClassLibrary.Models.Difficulty.Hard:
                                easyBar.Opacity = 1.0;
                                normalBar.Opacity = 1.0;
                                hardBar.Opacity = 1.0;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating difficulty indicator: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Event args for flashcard exercise send event.
    /// </summary>
    public class FlashcardExerciseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the user answer or content pairs.
        /// </summary>
        public string ContentPairs { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlashcardExerciseEventArgs"/> class.
        /// </summary>
        /// <param name="contentPairs">The user answer or content pairs.</param>
        public FlashcardExerciseEventArgs(string contentPairs)
        {
            this.ContentPairs = contentPairs;
        }
    }
}