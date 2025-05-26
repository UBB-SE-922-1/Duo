// <copyright file="SignUpPage.xaml.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.Views.Pages
{
    using System;
    using System.Threading.Tasks;
    using Duo.ViewModels;
    using DuoClassLibrary.Models;
    using DuolingoNou.Views;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// Page for user sign-up functionality.
    /// </summary>
    public sealed partial class SignUpPage : Page
    {
        /// <summary>
        /// Gets the ViewModel for this page.
        /// </summary>
        public SignUpViewModel ViewModel { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignUpPage"/> class.
        /// </summary>
        public SignUpPage()
        {
            this.InitializeComponent();
            this.ViewModel = App.ServiceProvider.GetRequiredService<SignUpViewModel>();
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// Handles the create user button click event.
        /// </summary>
        private async void OnCreateUserClick(object sender, RoutedEventArgs e)
        {
            this.ViewModel.NewUser.UserName = this.UsernameTextBox.Text;
            this.ViewModel.NewUser.Email = this.EmailTextBox.Text;
            this.ViewModel.NewUser.Password = this.PasswordBoxWithRevealMode.Password;
            this.ViewModel.ConfirmPassword = this.ConfirmPasswordBox.Password;

            // Validate username format
            if (!this.ViewModel.ValidateUsername(this.ViewModel.NewUser.UserName))
            {
                this.UsernameValidationTextBlock.Text = this.ViewModel.UsernameValidationMessage;
                return;
            }

            // Check if username is already taken
            if (await this.ViewModel.IsUsernameTaken(this.ViewModel.NewUser.UserName))
            {
                await this.ShowDialog("Username Taken", "This username is already in use. Please choose another.");
                return;
            }

            // Update and validate password strength
            this.ViewModel.UpdatePasswordStrength(this.ViewModel.NewUser.Password);
            if (this.ViewModel.PasswordStrength == "Weak")
            {
                await this.ShowDialog("Weak Password", "Password must be at least Medium strength. Include an uppercase letter, a special character, and a digit.");
                return;
            }

            // Validate password match
            if (!this.ViewModel.ValidatePasswordMatch())
            {
                this.ConfirmPasswordValidationTextBlock.Text = this.ViewModel.ConfirmPasswordValidationMessage;
                return;
            }

            // Create the user
            bool success = await this.ViewModel.CreateNewUser(this.ViewModel.NewUser);

            if (success)
            {
                // Set the current user globally
                App.CurrentUser = this.ViewModel.NewUser;

                await this.ShowDialog("Account Created", "Your account has been successfully created!");
                await App.UserService.SetUser(App.CurrentUser.UserName);

                this.Frame.Navigate(typeof(CategoryPage));
            }
            else
            {
                await this.ShowDialog("Error", "There was a problem creating your account. Please try again.");
            }
        }

        /// <summary>
        /// Displays a dialog with the specified title and message.
        /// </summary>
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

        /// <summary>
        /// Handles the password changes and updates the strength indicator.
        /// </summary>
        private void PasswordBoxWithRevealMode_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.ViewModel.UpdatePasswordStrength(this.PasswordBoxWithRevealMode.Password);
            this.PasswordStrengthTextBlock.Text = this.ViewModel.PasswordStrength;
        }

        /// <summary>
        /// Handles the reveal mode checkbox changes.
        /// </summary>
        private void RevealModeCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            this.PasswordBoxWithRevealMode.PasswordRevealMode = this.RevealModeCheckBox.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
            this.ConfirmPasswordBox.PasswordRevealMode = this.RevealModeCheckBox.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
        }

        /// <summary>
        /// Navigates to the login page.
        /// </summary>
        private void NavigateToLoginPage(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage));
        }
    }
}