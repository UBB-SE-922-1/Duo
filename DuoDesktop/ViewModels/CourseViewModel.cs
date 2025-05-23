using DuoClassLibrary.Models;
using DuoClassLibrary.Services;
using DuoClassLibrary.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DuoDesktop.ViewModels
{
    public class CourseViewModel : INotifyPropertyChanged
    {
        internal const int NotificationDisplayDurationInSeconds = 3;
        private string notificationMessageText = string.Empty;
        private bool shouldShowNotification = false;

        private readonly ICourseService _courseService;
        private ObservableCollection<Course> _enrolledCourses;

        public ObservableCollection<Course> EnrolledCourses
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
            EnrolledCourses = new ObservableCollection<Course>();
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

        public virtual string NotificationMessage
        {
            get => notificationMessageText;
            set
            {
                notificationMessageText = value;
                OnPropertyChanged(nameof(NotificationMessage));
            }
        }

        public virtual bool ShowNotification
        {
            get => shouldShowNotification;
            set
            {
                shouldShowNotification = value;
                OnPropertyChanged(nameof(ShowNotification));
            }
        }
    }
}