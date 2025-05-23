using Microsoft.UI.Xaml.Controls;
using DuoDesktop.ViewModels;

namespace DuoDesktop.Views.Pages
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