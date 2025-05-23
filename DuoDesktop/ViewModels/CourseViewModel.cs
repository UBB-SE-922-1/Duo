using DuolingoClassLibrary.Entities;
using DuolingoClassLibrary.Services;
using DuolingoClassLibrary.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DuolingoNou.ViewModels
{
    public class CourseViewModel : INotifyPropertyChanged
    {
        private readonly ICourseService _courseService;
        private ObservableCollection<MyCourse> _enrolledCourses;

        public ObservableCollection<MyCourse> EnrolledCourses
        {
            get => _enrolledCourses;
            set
            {
                _enrolledCourses = value;
                OnPropertyChanged();
            }
        }

        public CourseViewModel(ICourseService courseService = null)
        {
            _courseService = courseService ?? new CourseService();
            EnrolledCourses = new ObservableCollection<MyCourse>();
            LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            var courses = await _courseService.GetEnrolledCoursesAsync();
            foreach (var course in courses)
            {
                EnrolledCourses.Add(course);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}