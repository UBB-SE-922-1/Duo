using DuolingoClassLibrary.Entities;
using DuolingoClassLibrary.Services;
using DuolingoClassLibrary.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DuolingoNou.ViewModels
{
    public class QuizViewModel : INotifyPropertyChanged
    {
        private readonly IQuizService _quizService;
        private ObservableCollection<Quiz> _completedQuizzes;

        public ObservableCollection<Quiz> CompletedQuizzes
        {
            get => _completedQuizzes;
            set
            {
                _completedQuizzes = value;
                OnPropertyChanged();
            }
        }

        public QuizViewModel(IQuizService quizService = null)
        {
            _quizService = quizService ?? new QuizService();
            CompletedQuizzes = new ObservableCollection<Quiz>();
            LoadQuizzesAsync();
        }

        private async Task LoadQuizzesAsync()
        {
            var quizzes = await _quizService.GetCompletedQuizzesAsync();
            foreach (var quiz in quizzes)
            {
                CompletedQuizzes.Add(quiz);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}