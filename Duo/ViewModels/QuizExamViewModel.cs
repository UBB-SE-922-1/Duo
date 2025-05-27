// <copyright file="QuizExamViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Duo.ViewModels.Base;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Models.Exercises;
    using DuoClassLibrary.Models.Quizzes;
    using DuoClassLibrary.Models.Sections;
    using DuoClassLibrary.Services;
    using DuoClassLibrary.Services.Interfaces;

    /// <summary>
    /// ViewModel for handling quiz and exam logic, including exercise navigation, validation, and user progression.
    /// </summary>
    public class QuizExamViewModel : ViewModelBase
    {
        private readonly IExerciseService exerciseService;
        private readonly IQuizService quizService;
        private int quizId;
        private int examId;
        private Quiz? currentQuiz;
        private Exam? currentExam;
        private List<Exercise> exercises = new List<Exercise>();
        private Exercise? currentExercise;
        private int currentExerciseIndex;
        private bool? validatedCurrent = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuizExamViewModel"/> class with DI.
        /// </summary>
        /// <param name="exerciseService">The exercise service.</param>
        public QuizExamViewModel(IExerciseService exerciseService)
        {
            try
            {
                this.exerciseService = exerciseService ?? throw new ArgumentNullException(nameof(exerciseService));
                this.quizService = null!;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Initialization Error", $"Failed to initialize QuizExamViewModel.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuizExamViewModel"/> class using service provider.
        /// </summary>
        public QuizExamViewModel()
        {
            try
            {
                this.exerciseService = (IExerciseService)(App.ServiceProvider?.GetService(typeof(IExerciseService)) ?? throw new InvalidOperationException("ExerciseService not found."));
                this.quizService = (IQuizService)(App.ServiceProvider?.GetService(typeof(IQuizService)) ?? throw new InvalidOperationException("QuizService not found."));
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Initialization Error", $"Failed to initialize QuizExamViewModel.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the quiz ID.
        /// </summary>
        public int QuizId
        {
            get => this.quizId;
            private set
            {
                this.quizId = value;
                this.OnPropertyChanged(nameof(this.QuizId));
            }
        }

        /// <summary>
        /// Gets the exam ID.
        /// </summary>
        public int ExamId
        {
            get => this.examId;
            private set
            {
                this.examId = value;
                this.OnPropertyChanged(nameof(this.ExamId));
            }
        }

        /// <summary>
        /// Gets the list of exercises.
        /// </summary>
        public List<Exercise> Exercises
        {
            get => this.exercises;
            private set
            {
                this.exercises = value;
                this.OnPropertyChanged(nameof(this.Exercises));
            }
        }

        /// <summary>
        /// Gets or sets the current exercise.
        /// </summary>
        public Exercise? CurrentExercise
        {
            get => this.currentExercise;
            set
            {
                this.currentExercise = value;
                this.OnPropertyChanged(nameof(this.CurrentExercise));
            }
        }

        /// <summary>
        /// Gets or sets the current quiz.
        /// </summary>
        public Quiz? CurrentQuiz
        {
            get => this.currentQuiz;
            set
            {
                this.currentQuiz = value;
                this.OnPropertyChanged(nameof(this.CurrentQuiz));
            }
        }

        /// <summary>
        /// Gets or sets the current exam.
        /// </summary>
        public Exam? CurrentExam
        {
            get => this.currentExam;
            set
            {
                this.currentExam = value;
                this.OnPropertyChanged(nameof(this.CurrentExam));
            }
        }

        /// <summary>
        /// Gets or sets the index of the current exercise.
        /// </summary>
        public int CurrentExerciseIndex
        {
            get => this.currentExerciseIndex;
            set
            {
                this.currentExerciseIndex = value;
                this.OnPropertyChanged(nameof(this.CurrentExerciseIndex));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current exercise is validated.
        /// </summary>
        public bool? ValidatedCurrent
        {
            get => this.validatedCurrent;
            set
            {
                this.validatedCurrent = value;
                this.OnPropertyChanged(nameof(this.ValidatedCurrent));
            }
        }

        /// <summary>
        /// Gets the correct answers text for the current quiz or exam.
        /// </summary>
        public string CorrectAnswersText
        {
            get
            {
                try
                {
                    if (this.QuizId != -1)
                    {
                        return $"{this.CurrentQuiz?.GetNumberOfCorrectAnswers() ?? 0}/{this.CurrentQuiz?.GetNumberOfAnswersGiven() ?? 0}";
                    }

                    return $"{this.CurrentExam?.GetNumberOfCorrectAnswers() ?? 0}/{this.CurrentExam?.GetNumberOfAnswersGiven() ?? 0}";
                }
                catch (Exception ex)
                {
                    this.RaiseErrorMessage("Correct Answers Error", $"Failed to get correct answers text.\nDetails: {ex.Message}");
                    return "0/0";
                }
            }
        }

        /// <summary>
        /// Gets the passing percent text for the current quiz or exam.
        /// </summary>
        public string PassingPercentText
        {
            get
            {
                try
                {
                    return $"{(int)Math.Round(this.GetPercentageCorrect() * 100)}%";
                }
                catch (Exception ex)
                {
                    this.RaiseErrorMessage("Passing Percent Error", $"Failed to get passing percent text.\nDetails: {ex.Message}");
                    return "0%";
                }
            }
        }

        /// <summary>
        /// Gets the pass/fail status text for the current quiz or exam.
        /// </summary>
        public string IsPassedText
        {
            get
            {
                try
                {
                    if (this.IsPassed())
                    {
                        return "Great job! You passed this one.";
                    }

                    return "You need to redo this one.";
                }
                catch (Exception ex)
                {
                    this.RaiseErrorMessage("Pass Status Error", $"Failed to get pass status text.\nDetails: {ex.Message}");
                    return "Error determining pass status.";
                }
            }
        }

        /// <summary>
        /// Sets the quiz ID and loads quiz data.
        /// </summary>
        /// <param name="value">The quiz ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SetQuizIdAsync(int value)
        {
            try
            {
                this.QuizId = value;
                this.ExamId = -1;
                await this.LoadQuizData();
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Set Quiz Error", $"Failed to set quiz ID and load data.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the exam ID and loads exam data.
        /// </summary>
        /// <param name="value">The exam ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SetExamIdAsync(int value)
        {
            try
            {
                this.ExamId = value;
                this.QuizId = -1;
                await this.LoadExamData();
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Set Exam Error", $"Failed to set exam ID and load data.\nDetails: {ex.Message}");
            }
        }

        private async Task LoadQuizData()
        {
            try
            {
                await this.LoadQuiz();
                await this.LoadExercises();
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Load Quiz Data Error", $"Failed to load quiz data.\nDetails: {ex.Message}");
            }
        }

        private async Task LoadExamData()
        {
            try
            {
                await this.LoadExam();
                await this.LoadExercises();
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Load Exam Data Error", $"Failed to load exam data.\nDetails: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the quiz by ID.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadQuiz()
        {
            try
            {
                this.CurrentQuiz = await this.quizService.GetQuizById(this.QuizId)
                    ?? throw new InvalidOperationException("Quiz not found.");
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Load Quiz Error", $"Failed to load quiz with ID {this.QuizId}.\nDetails: {ex.Message}");
                this.CurrentQuiz = null;
            }
        }

        /// <summary>
        /// Loads the exam by ID.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadExam()
        {
            try
            {
                this.CurrentExam = await this.quizService.GetExamById(this.ExamId)
                    ?? throw new InvalidOperationException("Exam not found.");
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Load Exam Error", $"Failed to load exam with ID {this.ExamId}.\nDetails: {ex.Message}");
                this.CurrentExam = null;
            }
        }

        /// <summary>
        /// Loads the exercises for the current quiz or exam.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadExercises()
        {
            try
            {
                if (this.QuizId != -1)
                {
                    this.Exercises = await this.exerciseService.GetAllExercisesFromQuiz(this.QuizId)
                        ?? new List<Exercise>();
                    if (this.CurrentQuiz != null)
                    {
                        this.CurrentQuiz.Exercises = this.Exercises;
                    }
                }
                else
                {
                    this.Exercises = await this.exerciseService.GetAllExercisesFromExam(this.ExamId)
                        ?? new List<Exercise>();
                    if (this.CurrentExam != null)
                    {
                        this.CurrentExam.Exercises = this.Exercises;
                    }
                }

                this.CurrentExerciseIndex = 0;
                this.CurrentExercise = this.Exercises.Any() ? this.Exercises[this.CurrentExerciseIndex] : null;
                this.ValidatedCurrent = null;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Load Exercises Error", $"Failed to load exercises.\nDetails: {ex.Message}");
                this.Exercises = new List<Exercise>();
                this.CurrentExercise = null;
            }
        }

        /// <summary>
        /// Validates the current exercise with the provided responses.
        /// </summary>
        /// <param name="responses">The user responses.</param>
        /// <returns>True if the answer is correct; otherwise, false.</returns>
        public bool? ValidateCurrentExercise(object responses)
        {
            try
            {
                if (this.ValidatedCurrent is not null)
                {
                    return this.ValidatedCurrent;
                }

                bool isValid = false;

                if (this.CurrentExercise is AssociationExercise associationExercise)
                {
                    isValid = associationExercise.ValidateAnswer((List<(string, string)>)responses);
                }
                else if (this.CurrentExercise is FillInTheBlankExercise fillInTheBlanksExercise)
                {
                    isValid = fillInTheBlanksExercise.ValidateAnswer((List<string>)responses);
                }
                else if (this.CurrentExercise is MultipleChoiceExercise multipleChoiceExercise)
                {
                    isValid = multipleChoiceExercise.ValidateAnswer((List<string>)responses);
                }
                else if (this.CurrentExercise is FlashcardExercise flashcardExercise)
                {
                    isValid = flashcardExercise.ValidateAnswer((string)responses);
                }

                this.ValidatedCurrent = isValid;
                if (this.QuizId != -1)
                {
                    this.UpdateQuiz(this.ValidatedCurrent);
                }
                else
                {
                    this.UpdateExam(this.ValidatedCurrent);
                }

                return isValid;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Validate Exercise Error", $"Failed to validate current exercise.\nDetails: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads the next exercise in the list.
        /// </summary>
        /// <returns>True if the next exercise was loaded; otherwise, false.</returns>
        public bool LoadNext()
        {
            try
            {
                if (this.ValidatedCurrent == null)
                {
                    return false;
                }

                this.CurrentExerciseIndex += 1;
                if (this.Exercises.Count <= this.CurrentExerciseIndex)
                {
                    this.CurrentExercise = null;
                }
                else
                {
                    this.CurrentExercise = this.Exercises[this.CurrentExerciseIndex];
                }

                this.ValidatedCurrent = null;
                return true;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Load Next Error", $"Failed to load next exercise.\nDetails: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the percentage of exercises completed.
        /// </summary>
        /// <returns>The percentage done as a float.</returns>
        public float GetPercentageDone()
        {
            try
            {
                if (this.QuizId != -1)
                {
                    return (float)(this.CurrentQuiz?.GetNumberOfAnswersGiven() ?? 0) / (this.Exercises.Count == 0 ? 1 : this.Exercises.Count);
                }

                return (float)(this.CurrentExam?.GetNumberOfAnswersGiven() ?? 0) / (this.Exercises.Count == 0 ? 1 : this.Exercises.Count);
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Percentage Done Error", $"Failed to calculate percentage done.\nDetails: {ex.Message}");
                return 0f;
            }
        }

        /// <summary>
        /// Gets the correct answer(s) for the current exercise.
        /// </summary>
        /// <returns>The correct answer(s) as a string.</returns>
        public string GetCurrentExerciseCorrectAnswer()
        {
            try
            {
                string correctAnswers = string.Empty;

                if (this.CurrentExercise is AssociationExercise associationExercise)
                {
                    for (int i = 0; i < associationExercise.FirstAnswersList.Count; i++)
                    {
                        correctAnswers += associationExercise.FirstAnswersList[i] + " - " + associationExercise.SecondAnswersList[i] + "\n";
                    }
                }
                else if (this.CurrentExercise is FillInTheBlankExercise fillInTheBlanksExercise)
                {
                    foreach (var answer in fillInTheBlanksExercise.PossibleCorrectAnswers)
                    {
                        correctAnswers += answer + "\n";
                    }
                }
                else if (this.CurrentExercise is MultipleChoiceExercise multipleChoiceExercise)
                {
                    if (multipleChoiceExercise.Choices != null)
                    {
                        correctAnswers += multipleChoiceExercise.Choices
                            .Where(choice => choice.IsCorrect)
                            .Select(choice => choice.Answer)
                            .FirstOrDefault() ?? string.Empty;
                    }
                }
                else if (this.CurrentExercise is FlashcardExercise flashcardExercise)
                {
                    correctAnswers += string.Join(", ", flashcardExercise.GetCorrectAnswer());
                }

                return correctAnswers;
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Correct Answer Error", $"Failed to get correct answer.\nDetails: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Marks the user's progression after completing a quiz or exam.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task MarkUserProgression()
        {
            try
            {
                if (this.GetPercentageDone() != 1)
                {
                    return;
                }

                if (!this.IsPassed())
                {
                    return;
                }

                IUserService userService = (IUserService)(App.ServiceProvider?.GetService(typeof(IUserService)) ?? throw new InvalidOperationException("IUserService not found."));
                ISectionService sectionService = (ISectionService)(App.ServiceProvider?.GetService(typeof(ISectionService)) ?? throw new InvalidOperationException("ISectionService not found."));

                User user = userService.GetCurrentUser()
                    ?? throw new InvalidOperationException("User not found.");
                List<Section> sections = await sectionService.GetByRoadmapId(1)
                    ?? throw new InvalidOperationException("Sections not found.");

                int sectionId;
                if (this.QuizId == -1)
                {
                    sectionId = this.CurrentExam?.SectionId ?? -1;
                }
                else
                {
                    sectionId = this.CurrentQuiz?.SectionId ?? -1;
                }
                Section currentUserSection = await sectionService.GetSectionById(sectionId)
                    ?? throw new InvalidOperationException("Current user section not found.");
                List<Quiz> currentSectionQuizzes = await this.quizService.GetAllQuizzesFromSection(currentUserSection.Id)
                    ?? throw new InvalidOperationException("Quizzes not found.");

                if (this.QuizId == -1)
                {
                    if (this.CurrentExam == null)
                    {
                        this.RaiseErrorMessage("Progression Error", "Current exam is not set.");
                        return;
                    }
                    foreach (Quiz q in currentSectionQuizzes)
                    {
                        bool isCompleted = await this.quizService.IsQuizCompleted(user.UserId, q.Id);
                        if (!isCompleted)
                        {
                            this.RaiseErrorMessage("Progression Error", "Not all quizzes in section completed.");
                            return;
                        }
                    }
                    if (currentUserSection.GetFinalExam()?.Id != this.ExamId)
                    {
                        this.RaiseErrorMessage("Progression Error", "Exam ID does not match section's final exam.");
                        return;
                    }
                    bool isExamCompleted = await this.quizService.IsExamCompleted(user.UserId, this.ExamId);
                    if (isExamCompleted)
                    {
                        this.RaiseErrorMessage("Progression Error", "Exam already completed.");
                        return;
                    }

                    if (this.CurrentExam.SectionId == null)
                    {
                        this.RaiseErrorMessage("Error", "Exam has no section Id");
                        return;
                    }
                    else
                    {
                        await this.quizService.CompleteExam(user.UserId, this.ExamId);
                        await sectionService.CompleteSection(user.UserId, sectionId);
                    }
                }
                else
                {
                    if (this.CurrentQuiz == null)
                    {
                        this.RaiseErrorMessage("Progression Error", "Current quiz is not set.");
                        return;
                    }
                    int ct = 0;
                    foreach (Quiz q in currentSectionQuizzes)
                    {
                        bool isCompleted = await this.quizService.IsQuizCompleted(user.UserId, q.Id);
                        if (isCompleted)
                        {
                            ct++;
                        }
                    }
                    if (ct == currentSectionQuizzes.Count)
                    {
                        this.RaiseErrorMessage("Progression Error", "All quizzes already completed.");
                        return;
                    }
                    if (this.CurrentQuiz.Id != this.QuizId)
                    {
                        this.RaiseErrorMessage("Progression Error", "Quiz ID does not match expected quiz.");
                        return;
                    }
                    await this.quizService.CompleteQuiz(user.UserId, this.QuizId);
                }
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Mark Progression Error", $"Failed to mark user progression.\nDetails: {ex.Message}");
            }
        }

        private void UpdateQuiz(bool? isExerciseValid)
        {
            try
            {
                if (isExerciseValid == true && this.CurrentQuiz != null)
                {
                    this.CurrentQuiz.IncrementCorrectAnswers();
                }
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Update Quiz Error", $"Failed to update quiz progress.\nDetails: {ex.Message}");
            }
        }

        private void UpdateExam(bool? isExerciseValid)
        {
            try
            {
                if (isExerciseValid == true && this.CurrentExam != null)
                {
                    this.CurrentExam.IncrementCorrectAnswers();
                }
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Update Exam Error", $"Failed to update exam progress.\nDetails: {ex.Message}");
            }
        }

        private float GetPercentageCorrect()
        {
            try
            {
                if (this.QuizId != -1)
                {
                    return (float)(this.CurrentQuiz?.GetNumberOfCorrectAnswers() ?? 0) / (this.Exercises.Count == 0 ? 1 : this.Exercises.Count);
                }

                return (float)(this.CurrentExam?.GetNumberOfCorrectAnswers() ?? 0) / (this.Exercises.Count == 0 ? 1 : this.Exercises.Count);
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Percentage Correct Error", $"Failed to calculate percentage correct.\nDetails: {ex.Message}");
                return 0f;
            }
        }

        private bool IsPassed()
        {
            try
            {
                int percentCorrect = (int)Math.Round(this.GetPercentageCorrect() * 100);
                if (this.QuizId != -1)
                {
                    return percentCorrect >= (this.CurrentQuiz?.GetPassingThreshold() ?? 0);
                }

                return percentCorrect >= (this.CurrentExam?.GetPassingThreshold() ?? 0);
            }
            catch (Exception ex)
            {
                this.RaiseErrorMessage("Pass Check Error", $"Failed to determine pass status.\nDetails: {ex.Message}");
                return false;
            }
        }
    }
}