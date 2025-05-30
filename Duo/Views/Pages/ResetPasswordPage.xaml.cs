// <copyright file="ResetPasswordPage.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace DuolingoNou.Views.Pages
{
    using System;
    using Duo;
    using Duo.ViewModels;
    using Duo.Views.Pages;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Data;

    /// <summary>
    /// Interaction logic for ResetPasswordPage.xaml.
    /// </summary>
    public sealed partial class ResetPasswordPage : Page
    {
        private readonly ResetPassViewModel resetPassViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetPasswordPage"/> class.
        /// </summary>
        public ResetPasswordPage()
        {
            this.InitializeComponent();

            // Get the ResetPassViewModel from DI container
            this.resetPassViewModel = App.ServiceProvider.GetRequiredService<ResetPassViewModel>();
            this.DataContext = this.resetPassViewModel;

            // Set up bindings for visibility
            this.EmailPanel.SetBinding(
                VisibilityProperty,
                new Binding
                {
                    Path = new PropertyPath("EmailPanelVisible"),
                    Source = this.resetPassViewModel,
                    Converter = (IValueConverter)this.Resources["BooleanToVisibilityConverter"],
                });

            this.CodePanel.SetBinding(
                VisibilityProperty,
                new Binding
                {
                    Path = new PropertyPath("CodePanelVisible"),
                    Source = this.resetPassViewModel,
                    Converter = (IValueConverter)this.Resources["BooleanToVisibilityConverter"],
                });

            this.PasswordPanel.SetBinding(
                VisibilityProperty,
                new Binding
                {
                    Path = new PropertyPath("PasswordPanelVisible"),
                    Source = this.resetPassViewModel,
                    Converter = (IValueConverter)this.Resources["BooleanToVisibilityConverter"],
                });
        }

        /// <summary>
        /// Handles the send verification code button click.
        /// </summary>
        private async void OnSendCodeClick(object sender, RoutedEventArgs e)
        {
            string email = this.EmailTextBox.Text;

            // Disable button while processing
            if (sender is Button button)
            {
                button.IsEnabled = false;
            }

            await this.resetPassViewModel.SendVerificationCode(email);

            // Re-enable button if failed
            if (!this.resetPassViewModel.CodePanelVisible && sender is Button btn)
            {
                btn.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the verify code button click.
        /// </summary>
        private async void OnVerifyCodeClick(object sender, RoutedEventArgs e)
        {
            string code = this.CodeTextBox.Text;
            await this.resetPassViewModel.VerifyCode(code);
        }

        /// <summary>
        /// Handles the reset password button click.
        /// </summary>
        private async void OnResetPasswordClick(object sender, RoutedEventArgs e)
        {
            this.resetPassViewModel.NewPassword = this.NewPasswordBox.Password;
            this.resetPassViewModel.ConfirmPassword = this.ConfirmPasswordBox.Password;

            bool isReset = await this.resetPassViewModel.ResetPassword(this.resetPassViewModel.NewPassword);

            if (isReset)
            {
                // Navigate back to login page
                this.Frame.Navigate(typeof(LoginPage));
            }
        }

        /// <summary>
        /// Handles the back button click.
        /// </summary>
        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(LoginPage));
        }
    }
}