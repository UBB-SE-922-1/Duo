using Duo.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

namespace Duo.Views.Pages
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var userName = App.CurrentUser?.UserName ?? "User";
            WelcomeTextBlock.Text = $"Welcome, {userName}!";
        }

        /// <summary>
        /// Logout button handler
        /// </summary>
        public void LogoutButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ((MainPageViewModel)App.ServiceProvider.GetService(typeof(MainPageViewModel))).HandleLogoutClick();
            // get parent frame
            Frame parentFrame = this.Frame;
            if (parentFrame != null)
            {
                // Navigate to LoginPage
                parentFrame.Navigate(typeof(LoginPage));
            }
            else
            {
                Debug.WriteLine("Parent frame is null, cannot navigate to LoginPage.");
            }
        }
    }
}
