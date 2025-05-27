// <copyright file="CreateSectionViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.ViewModels.Base;
    using DuoClassLibrary.Models.Exercises;
    using DuoClassLibrary.Models.Quizzes;
    using DuoClassLibrary.Models.Sections;
    using DuoClassLibrary.Services;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// ViewModel for creating a section, including quizzes and exams.
    /// </summary>
    internal partial class CreateSectionViewModel : AdminBaseViewModel
    {
        private readonly ISectionService sectionService;
        private readonly IQuizService quizService;
        private readonly IExerciseService exerciseService;
        private string subjectText = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSectionViewModel"/> class.
        /// </summary>
        public CreateSectionViewModel()
        {
            try
            {
                this.sectionService = (ISectionService)(App.ServiceProvider.GetService(typeof(ISectionService)) ?? throw new InvalidOperationException("SectionService not found."));
                this.quizService = (IQuizService)(App.ServiceProvider.GetService(typeof(IQuizService)) ?? throw new InvalidOperationException("QuizService not found."));
                this.exerciseService = (IExerciseService)(App.ServiceProvider.GetService(typeof(IExerciseService)) ?? throw new InvalidOperationException("ExerciseService not found."));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Initialization error", ex.Message);
            }

            // OpenSelectQuizesCommand = new RelayCommand(OpenSelectQuizes);

            // Update the RelayCommand initialization to use a lambda expression that matches the expected Func<object?, Task> signature.

            this.OpenSelectQuizesCommand = new RelayCommand(_ => this.OpenSelectQuizes());
            this.OpenSelectExamsCommand = new RelayCommand(_ => this.OpenSelectExams());
            this.SaveButtonCommand = new RelayCommand(_ => this.CreateSection());
            this.SaveButtonCommand = new RelayCommand(_ => _ = this.CreateSection());

            this.RemoveQuizCommand = new RelayCommandWithParameter<Quiz>(this.RemoveSelectedQuiz);
            this.RemoveExamCommand = new RelayCommandWithParameter<Exam>(this.RemoveSelectedExam);

            _ = this.GetQuizesAsync();
            _ = this.GetExamsAsync();
        }

        /// <summary>
        /// Gets or sets the collection of quizzes.
        /// </summary>
        public ObservableCollection<Quiz> Quizes { get; set; } = new ObservableCollection<Quiz>();

        /// <summary>
        /// Gets the collection of selected quizzes.
        /// </summary>
        public ObservableCollection<Quiz> SelectedQuizes { get; private set; } = new ObservableCollection<Quiz>();

        /// <summary>
        /// Gets or sets the collection of exams.
        /// </summary>
        public ObservableCollection<Exam> Exams { get; set; } = new ObservableCollection<Exam>();

        /// <summary>
        /// Gets the collection of selected exams.
        /// </summary>
        public ObservableCollection<Exam> SelectedExams { get; private set; } = new ObservableCollection<Exam>();

        /// <summary>
        /// Occurs when the quiz list view modal should be shown.
        /// </summary>
        public event Action<List<Quiz>>? ShowListViewModalQuizes;

        /// <summary>
        /// Occurs when the exam list view modal should be shown.
        /// </summary>
        public event Action<List<Exam>>? ShowListViewModalExams;

        /// <summary>
        /// Gets the command to remove a quiz.
        /// </summary>
        public ICommand RemoveQuizCommand { get; }

        /// <summary>
        /// Gets the command to remove an exam.
        /// </summary>
        public ICommand RemoveExamCommand { get; }

        /// <summary>
        /// Gets the command to save the section.
        /// </summary>
        public ICommand SaveButtonCommand { get; }

        /// <summary>
        /// Gets the command to open the select quizzes dialog.
        /// </summary>
        public ICommand OpenSelectQuizesCommand { get; }

        /// <summary>
        /// Gets the command to open the select exams dialog.
        /// </summary>
        public ICommand OpenSelectExamsCommand { get; }

        /// <summary>
        /// Gets or sets the subject text.
        /// </summary>
        public string SubjectText
        {
            get => this.subjectText;
            set
            {
                if (this.subjectText != value)
                {
                    this.subjectText = value;
                    this.OnPropertyChanged(nameof(this.SubjectText));
                }
            }
        }

        /// <summary>
        /// Removes the selected quiz.
        /// </summary>
        /// <param name="quizToBeRemoved">The quiz to remove.</param>
        public void RemoveSelectedQuiz(Quiz quizToBeRemoved)
        {
            try
            {
                Debug.WriteLine("Removing quiz...");
                this.SelectedQuizes.Remove(quizToBeRemoved);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to remove quiz", ex.Message);
            }
        }

        /// <summary>
        /// Removes the selected exam.
        /// </summary>
        /// <param name="examToBeRemoved">The exam to remove.</param>
        public void RemoveSelectedExam(Exam examToBeRemoved)
        {
            try
            {
                Debug.WriteLine("Removing quiz...");
                this.SelectedExams.Remove(examToBeRemoved);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to remove quiz", ex.Message);
            }
        }

        /// <summary>
        /// Opens the select quizzes dialog.
        /// </summary>
        public void OpenSelectQuizes()
        {
            try
            {
                Debug.WriteLine("Opening select quizes...");
                this.ShowListViewModalQuizes?.Invoke(this.GetAvailableQuizes());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to open quiz selection", ex.Message);
            }
        }

        /// <summary>
        /// Opens the select exams dialog.
        /// </summary>
        public void OpenSelectExams()
        {
            try
            {
                Debug.WriteLine("Opening select exams...");
                this.ShowListViewModalExams?.Invoke(this.GetAvailableExams());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to open exam selection", ex.Message);
            }
        }

        /// <summary>
        /// Gets the available quizzes.
        /// </summary>
        /// <returns>List of available quizzes.</returns>
        public List<Quiz> GetAvailableQuizes()
        {
            try
            {
                return this.Quizes.Where(quiz => !this.SelectedQuizes.Contains(quiz)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to get available quizzes", ex.Message);
                return new List<Quiz>();
            }
        }

        /// <summary>
        /// Gets the available exams.
        /// </summary>
        /// <returns>List of available exams.</returns>
        public List<Exam> GetAvailableExams()
        {
            try
            {
                return this.Exams.Where(exam => !this.SelectedExams.Contains(exam)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to get available exams", ex.Message);
                return new List<Exam>();
            }
        }

        /// <summary>
        /// Adds a quiz to the selected quizzes.
        /// </summary>
        /// <param name="newQuiz">The quiz to add.</param>
        public void AddQuiz(Quiz newQuiz)
        {
            try
            {
                if (newQuiz == null)
                {
                    this.RaiseErrorMessage("Quiz is null", "Cannot add a null quiz.");
                    return;
                }

                if (!this.SelectedQuizes.Contains(newQuiz))
                {
                    Debug.WriteLine("Adding quiz..." + newQuiz.Id);
                    this.SelectedQuizes.Add(newQuiz);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to add quiz", ex.Message);
            }
        }

        /// <summary>
        /// Adds an exam to the selected exams.
        /// </summary>
        /// <param name="newExam">The exam to add.</param>
        public void AddExam(Exam newExam)
        {
            try
            {
                if (newExam == null)
                {
                    this.RaiseErrorMessage("Exam is null", "Cannot add a null exam.");
                    return;
                }

                if (!this.SelectedExams.Contains(newExam))
                {
                    Debug.WriteLine("Adding exam..." + newExam.Id);
                    this.SelectedExams.Clear();
                    this.SelectedExams.Add(newExam);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Failed to add exam", ex.Message);
            }
        }

        /// <summary>
        /// Gets all available quizzes asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetQuizesAsync()
        {
            try
            {
                this.Quizes.Clear();
                List<Quiz> quizes = await this.quizService.GetAllAvailableQuizzes();
                foreach (var quiz in quizes)
                {
                    Debug.WriteLine(quiz);
                    this.Quizes.Add(quiz);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during GetQuizesAsync: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                this.RaiseErrorMessage("Failed to fetch quizzes", ex.Message);
            }
        }

        /// <summary>
        /// Gets all available exams asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GetExamsAsync()
        {
            try
            {
                this.Exams.Clear();
                List<Exam> exams = await this.quizService.GetAllAvailableExams();
                foreach (var exam in exams)
                {
                    this.Exams.Add(exam);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during GetExamAsync: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                this.RaiseErrorMessage("Failed to fetch exam", ex.Message);
            }
        }

        /// <summary>
        /// Creates a new section with the selected quizzes and exam.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CreateSection()
        {
            try
            {
                if (this.SubjectText.IsNullOrEmpty())
                {
                    throw new Exception("Missing Title");
                }

                Section newSection = new Section(0, 1, this.SubjectText, "placeholder description", 1, null);
                newSection.Quizzes = this.SelectedQuizes.ToList();
                foreach (var quiz in newSection.Quizzes)
                {
                    quiz.Exercises = await this.exerciseService.GetAllExercisesFromQuiz(quiz.Id);
                }

                foreach (var quiz in newSection.Quizzes)
                {
                    Debug.WriteLine(quiz);
                }

                if (this.SelectedExams.Count != 1)
                {
                    this.RaiseErrorMessage("You must have exactly one exam selected!", string.Empty);
                    return;
                }

                newSection.Exam = this.SelectedExams.ToList()[0];
                newSection.Exam.Exercises = await this.exerciseService.GetAllExercisesFromExam(newSection.Exam.Id);
                int sectionId = await this.sectionService.AddSection(newSection);

                /*foreach (var quiz in this.SelectedQuizes.ToList())
                {
                    quiz.SectionId = sectionId;
                    await this.quizService.UpdateQuiz(quiz);
                }*/

                Debug.WriteLine("Section created: " + newSection);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage(ex.Message, string.Empty);
            }

            this.GoBack();
        }
    }
}