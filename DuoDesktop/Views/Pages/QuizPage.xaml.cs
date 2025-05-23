using Microsoft.UI.Xaml.Controls;
using DuolingoNou.ViewModels;

namespace DuolingoNou.Views.Pages
{
    public sealed partial class QuizPage : Page
    {
        public QuizViewModel ViewModel { get; }

        public QuizPage()
        {
            this.InitializeComponent();
            ViewModel = new QuizViewModel();
        }
    }
}