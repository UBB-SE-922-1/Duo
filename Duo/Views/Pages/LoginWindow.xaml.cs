// <copyright file="LoginWindow.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Duo.Services;
    using Duo.ViewModels;
    using DuoClassLibrary.Services.Interfaces;
    using DuolingoNou.Views;
    using DuolingoNou.Views.Pages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    /// <summary>
    /// Login page for user authentication.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private readonly IUserService userService;

        /// <summary>
        /// Gets the ViewModel for this page.
        /// </summary>
        public LoginViewModel ViewModel { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPage"/> class.
        /// </summary>
        public LoginPage()
        {
            this.InitializeComponent();

            // Get services from DI container
            var loginService = App.ServiceProvider.GetRequiredService<ILoginService>();
            this.userService = App.ServiceProvider.GetRequiredService<IUserService>();

            // Create ViewModel with injected service
            this.ViewModel = new LoginViewModel(loginService);

            this.DataContext = this.ViewModel;
        }

        /// <inheritdoc/>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Clear any cached/back-stacked CategoryPage so we get a fresh one next time
            var navFrame = this.Frame;
            for (int i = navFrame.BackStack.Count - 1; i >= 0; i--)
            {
                if (navFrame.BackStack[i].SourcePageType == typeof(CategoryPage))
                {
                    navFrame.BackStack.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Handles the password reveal mode checkbox change.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void RevealModeCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            this.PasswordBoxWithRevealMode.PasswordRevealMode =
                this.RevealModeCheckBox.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
        }

        /// <summary>
        /// Navigates to the sign-up page.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void NavigateToSignUpPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SignUpPage));
        }

        /// <summary>
        /// Handles the login button click.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private async void OnLoginButtonClick(object sender, RoutedEventArgs e)
        {
            string username = this.UsernameTextBox.Text;
            string password = this.PasswordBoxWithRevealMode.Password;

            bool loginSuccess = await this.ViewModel.AttemptLogin(username, password);

            if (loginSuccess)
            {
                App.CurrentUser = this.ViewModel.LoggedInUser;
                this.LoginStatusMessage.Text = "You have successfully logged in!";
                this.LoginStatusMessage.Visibility = Visibility.Visible;

                // Set the current user in the user service
                await this.userService.SetUser(this.ViewModel.LoggedInUser.UserName);
                await App.UserService.SetUser(this.ViewModel.LoggedInUser.UserName);

                App.MainAppWindow.Content = new CategoryPage();
            }
            else
            {
                this.LoginStatusMessage.Text = "Invalid username or password.";
                this.LoginStatusMessage.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Navigates to the reset password page.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnForgotPasswordClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ResetPasswordPage));
        }

        /// <summary>
        /// Shows a content dialog with the given title and message.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="content">The dialog content.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDialog(string title, string content)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
            };

            await dialog.ShowAsync();
        }
    }
}