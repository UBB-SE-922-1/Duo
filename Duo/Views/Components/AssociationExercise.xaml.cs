// <copyright file="AssociationExercise.xaml.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Duo.ViewModels.Base;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
    using Microsoft.UI.Xaml.Shapes;
    using Windows.Foundation;
    using Windows.UI;
    using Windows.UI.ViewManagement;

    /// <summary>
    /// UserControl for association (matching) exercises.
    /// </summary>
    public sealed partial class AssociationExercise : UserControl
    {
        /// <summary>
        /// Occurs when the send button is clicked.
        /// </summary>
        public event EventHandler<AssociationExerciseEventArgs>? OnSendClicked;

        /// <summary>
        /// Occurs when the control is clicked.
        /// </summary>
        public event RoutedEventHandler? Click;

        /// <summary>
        /// Identifies the Question dependency property.
        /// </summary>
        public static readonly DependencyProperty QuestionProperty =
            DependencyProperty.Register(nameof(Question), typeof(string), typeof(AssociationExercise), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the FirstAnswersList dependency property.
        /// </summary>
        public static readonly DependencyProperty FirstAnswersListProperty =
            DependencyProperty.Register(nameof(FirstAnswersList), typeof(ObservableCollection<string>), typeof(AssociationExercise), new PropertyMetadata(new ObservableCollection<string>()));

        /// <summary>
        /// Identifies the SecondAnswersList dependency property.
        /// </summary>
        public static readonly DependencyProperty SecondAnswersListProperty =
            DependencyProperty.Register(nameof(SecondAnswersList), typeof(ObservableCollection<string>), typeof(AssociationExercise), new PropertyMetadata(new ObservableCollection<string>()));

        private static readonly UISettings UiSettings = new UISettings();
        private readonly SolidColorBrush accentBrush = new SolidColorBrush(UiSettings.GetColorValue(UIColorType.Accent));
        private static readonly SolidColorBrush TransparentBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        private static readonly SolidColorBrush SelectedBrush = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
        private static readonly SolidColorBrush MappedBrush = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
        private static readonly SolidColorBrush DefaultBorderBrush = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
        private static readonly SolidColorBrush LineBrush = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));

        private Button? selectedLeftButton;
        private Button? selectedRightButton;
        private readonly List<Tuple<Button, Button, Line>> pairs = new List<Tuple<Button, Button, Line>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssociationExercise"/> class.
        /// </summary>
        public AssociationExercise()
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
                _ = this.ShowErrorMessage("Initialization Error", $"Failed to initialize AssociationExercise.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets or sets the question text.
        /// </summary>
        public string Question
        {
            get => (string)this.GetValue(QuestionProperty);
            set => this.SetValue(QuestionProperty, value);
        }

        /// <summary>
        /// Gets or sets the first answers list (shuffled).
        /// </summary>
        public ObservableCollection<string> FirstAnswersList
        {
            get
            {
                try
                {
                    var list = (ObservableCollection<string>)this.GetValue(FirstAnswersListProperty);
                    return new ObservableCollection<string>(list.OrderBy(_ => Guid.NewGuid()));
                }
                catch (Exception ex)
                {
                    _ = this.ShowErrorMessage("Answers Error", $"Failed to get FirstAnswersList.\nDetails: {ex.Message}");
                    return new ObservableCollection<string>();
                }
            }
            set => this.SetValue(FirstAnswersListProperty, value);
        }

        /// <summary>
        /// Gets or sets the second answers list.
        /// </summary>
        public ObservableCollection<string> SecondAnswersList
        {
            get => (ObservableCollection<string>)this.GetValue(SecondAnswersListProperty);
            set => this.SetValue(SecondAnswersListProperty, value);
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

        private void HandleOptionClick(ref Button? selectedButton, Button clickedButton)
        {
            try
            {
                if (selectedButton == clickedButton)
                {
                    selectedButton.Background = TransparentBrush;
                    selectedButton = null;
                }
                else if (selectedButton != clickedButton && selectedButton != null)
                {
                    selectedButton.Background = TransparentBrush;
                    selectedButton = clickedButton;
                    selectedButton.Background = this.accentBrush;
                    selectedButton.BorderBrush = this.accentBrush;
                }
                else
                {
                    selectedButton = clickedButton;
                    selectedButton.Background = this.accentBrush;
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Option Click Error", $"Failed to handle option click.\nDetails: {ex.Message}");
            }
        }

        private void DestroyExistingConnections(Button clickedButton)
        {
            try
            {
                foreach (var mapping in this.pairs.ToList())
                {
                    Button leftButtonContent = mapping.Item1;
                    Button rightButtonContent = mapping.Item2;
                    Line line = mapping.Item3;

                    if (leftButtonContent == clickedButton || rightButtonContent == clickedButton)
                    {
                        this.pairs.Remove(mapping);
                        leftButtonContent.Background = TransparentBrush;
                        rightButtonContent.Background = TransparentBrush;
                        clickedButton.Background = this.accentBrush;
                        this.LinesCanvas.Children.Remove(line);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Connection Error", $"Failed to destroy existing connections.\nDetails: {ex.Message}");
            }
        }

        private void CheckConnection()
        {
            try
            {
                if (this.selectedLeftButton == null || this.selectedRightButton == null)
                {
                    return;
                }

                var line = new Line
                {
                    Stroke = this.accentBrush,
                    StrokeThickness = 2,
                    X1 = this.GetCirclePosition(this.selectedLeftButton, true).X,
                    Y1 = this.GetCirclePosition(this.selectedLeftButton, true).Y,
                    X2 = this.GetCirclePosition(this.selectedRightButton, false).X,
                    Y2 = this.GetCirclePosition(this.selectedRightButton, false).Y,
                };

                this.LinesCanvas.Children.Add(line);
                this.pairs.Add(new Tuple<Button, Button, Line>(this.selectedLeftButton, this.selectedRightButton, line));

                this.selectedLeftButton.Background = this.accentBrush;
                this.selectedRightButton.Background = this.accentBrush;
                this.selectedLeftButton = null;
                this.selectedRightButton = null;
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Connection Error", $"Failed to check connection.\nDetails: {ex.Message}");
            }
        }

        private Point GetCirclePosition(Button button, bool isLeftCircle)
        {
            try
            {
                var transform = button.TransformToVisual(this.LinesCanvas);
                var buttonPosition = transform.TransformPoint(new Point(0, 0));
                var buttonCenterY = buttonPosition.Y + (button.ActualHeight / 2);

                var stackPanel = button.Parent as StackPanel;
                if (stackPanel != null)
                {
                    var circle = stackPanel.Children.OfType<Ellipse>().FirstOrDefault();
                    if (circle != null)
                    {
                        var circleTransform = circle.TransformToVisual(this.LinesCanvas);
                        var circlePosition = circleTransform.TransformPoint(new Point(0, 0));

                        return new Point(
                            circlePosition.X + (circle.ActualWidth / 2),
                            circlePosition.Y + (circle.ActualHeight / 2));
                    }
                }

                var circleX = isLeftCircle
                    ? buttonPosition.X + button.ActualWidth + 12
                    : buttonPosition.X - 12;

                return new Point(circleX, buttonCenterY);
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Position Error", $"Failed to get circle position.\nDetails: {ex.Message}");
                return new Point(0, 0);
            }
        }

        private void LeftOption_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button clickedButton)
                {
                    _ = this.ShowErrorMessage("Click Error", "Invalid button clicked.");
                    return;
                }
                this.HandleOptionClick(ref this.selectedLeftButton, clickedButton);
                this.DestroyExistingConnections(clickedButton);
                this.CheckConnection();
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Left Option Click Error", $"Failed to handle left option click.\nDetails: {ex.Message}");
            }
        }

        private void RightOption_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not Button clickedButton)
                {
                    _ = this.ShowErrorMessage("Click Error", "Invalid button clicked.");
                    return;
                }
                this.HandleOptionClick(ref this.selectedRightButton, clickedButton);
                this.DestroyExistingConnections(clickedButton);
                this.CheckConnection();
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Right Option Click Error", $"Failed to handle right option click.\nDetails: {ex.Message}");
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<(string, string)> contentPairs = this.pairs
                    .Select(mapping => (
                        mapping.Item1.Content?.ToString() ?? string.Empty,
                        mapping.Item2.Content?.ToString() ?? string.Empty))
                    .ToList();

                this.OnSendClicked?.Invoke(this, new AssociationExerciseEventArgs(contentPairs));
            }
            catch (Exception ex)
            {
                _ = this.ShowErrorMessage("Send Click Error", $"Failed to process send action.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Event args for association exercise send event.
        /// </summary>
        public class AssociationExerciseEventArgs : EventArgs
        {
            /// <summary>
            /// Gets the content pairs (user input).
            /// </summary>
            public List<(string, string)> ContentPairs { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="AssociationExerciseEventArgs"/> class.
            /// </summary>
            /// <param name="contentPairs">The content pairs.</param>
            public AssociationExerciseEventArgs(List<(string, string)> contentPairs)
            {
                this.ContentPairs = contentPairs;
            }
        }
    }
}