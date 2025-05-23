using Microsoft.UI.Xaml.Controls;
using DuolingoNou.ViewModels;

namespace DuolingoNou.Views.Pages
{
    public sealed partial class CoursePage : Page
    {
        public CourseViewModel ViewModel { get; }

        public CoursePage()
        {
            this.InitializeComponent();
            ViewModel = new CourseViewModel();
        }
    }
}