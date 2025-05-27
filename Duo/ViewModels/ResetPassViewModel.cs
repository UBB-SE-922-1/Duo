// <copyright file="ResetPassViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Duo.Services;
    using Duo.Validators;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Services.Interfaces;

    /// <summary>
    /// ViewModel for password reset functionality.
    /// </summary>
    public class ResetPassViewModel : INotifyPropertyChanged
    {
        private readonly ForgotPassService forgotPassService;
        private readonly PasswordResetValidator validator;
        private string email = string.Empty;
        private string verificationCode = string.Empty;
        private string newPassword = string.Empty;
        private string confirmPassword = string.Empty;
        private string statusMessage = string.Empty;
        private bool isCodeVerified = false;
        private bool isProcessing = false;
        private bool emailPanelVisible = true;
        private bool codePanelVisible = false;
        private bool passwordPanelVisible = false;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string Email
        {
            get => this.email;
            set
            {
                this.email = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the verification code.
        /// </summary>
        public string VerificationCode
        {
            get => this.verificationCode;
            set
            {
                this.verificationCode = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        public string NewPassword
        {
            get => this.newPassword;
            set
            {
                this.newPassword = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the confirmed password.
        /// </summary>
        public string ConfirmPassword
        {
            get => this.confirmPassword;
            set
            {
                this.confirmPassword = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage
        {
            get => this.statusMessage;
            set
            {
                this.statusMessage = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the code is verified.
        /// </summary>
        public bool IsCodeVerified
        {
            get => this.isCodeVerified;
            private set
            {
                this.isCodeVerified = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a process is running.
        /// </summary>
        public bool IsProcessing
        {
            get => this.isProcessing;
            set
            {
                this.isProcessing = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the email panel is visible.
        /// </summary>
        public bool EmailPanelVisible
        {
            get => this.emailPanelVisible;
            set
            {
                this.emailPanelVisible = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the code panel is visible.
        /// </summary>
        public bool CodePanelVisible
        {
            get => this.codePanelVisible;
            set
            {
                this.codePanelVisible = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the password panel is visible.
        /// </summary>
        public bool PasswordPanelVisible
        {
            get => this.passwordPanelVisible;
            set
            {
                this.passwordPanelVisible = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetPassViewModel"/> class.
        /// </summary>
        /// <param name="userHelperService">The user helper service.</param>
        public ResetPassViewModel(IUserHelperService userHelperService)
        {
            if (userHelperService == null)
            {
                throw new ArgumentNullException(nameof(userHelperService));
            }

            this.forgotPassService = new ForgotPassService(userHelperService);
            this.validator = new PasswordResetValidator();
        }

        /// <summary>
        /// Validates the provided email address.
        /// </summary>
        /// <param name="email">The email to validate.</param>
        /// <returns>True if the email is valid; otherwise, false.</returns>
        public bool ValidateEmail(string email)
        {
            bool isValid = PasswordResetValidator.IsValidEmail(email);
            if (!isValid)
            {
                this.StatusMessage = "Please enter a valid email address.";
            }

            return isValid;
        }

        /// <summary>
        /// Sends a verification code to the specified email.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <returns>True if the code was sent successfully; otherwise, false.</returns>
        public async Task<bool> SendVerificationCode(string email)
        {
            if (!this.ValidateEmail(email))
            {
                return false;
            }

            this.Email = email;
            this.IsProcessing = true;
            this.StatusMessage = "Sending verification code...";

            try
            {
                string code = await this.forgotPassService.SendVerificationCode(email);

                if (!string.IsNullOrEmpty(code))
                {
                    this.StatusMessage = "Verification code sent. Please check your email.";
                    this.EmailPanelVisible = false;
                    this.CodePanelVisible = true;
                    this.VerificationCode = code;
                    return true;
                }
                else
                {
                    this.StatusMessage = "Failed to send verification code. Please try again.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.StatusMessage = $"An error occurred: {ex.Message}";
                return false;
            }
            finally
            {
                this.IsProcessing = false;
            }
        }

        /// <summary>
        /// Validates the verification code format.
        /// </summary>
        /// <param name="code">The code to validate.</param>
        /// <returns>True if the code is valid; otherwise, false.</returns>
        public bool ValidateCodeFormat(string code)
        {
            bool isValid = PasswordResetValidator.IsValidVerificationCode(code);
            if (!isValid)
            {
                this.StatusMessage = "Please enter the verification code.";
            }

            return isValid;
        }

        /// <summary>
        /// Verifies the specified code.
        /// </summary>
        /// <param name="code">The verification code.</param>
        /// <returns>True if the code is valid; otherwise, false.</returns>
        public Task<bool> VerifyCode(string code)
        {
            if (!this.ValidateCodeFormat(code))
            {
                return Task.FromResult(false);
            }

            this.VerificationCode = code;
            this.IsCodeVerified = this.forgotPassService.VerifyCode(code);

            if (this.IsCodeVerified)
            {
                this.StatusMessage = "Code verified. Please enter your new password.";
                this.CodePanelVisible = false;
                this.PasswordPanelVisible = true;
            }
            else
            {
                this.StatusMessage = "Invalid verification code. Please try again.";
            }

            return Task.FromResult(this.IsCodeVerified);
        }

        /// <summary>
        /// Validates if the passwords match.
        /// </summary>
        /// <returns>True if the passwords match; otherwise, false.</returns>
        public bool ValidatePasswordsMatch()
        {
            bool match = PasswordResetValidator.DoPasswordsMatch(this.NewPassword, this.ConfirmPassword);
            if (!match)
            {
                this.StatusMessage = "Passwords don't match!";
            }

            return match;
        }

        /// <summary>
        /// Validates if the new password is valid.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>True if the password is valid; otherwise, false.</returns>
        public bool ValidateNewPassword(string password)
        {
            bool isValid = PasswordResetValidator.IsValidNewPassword(password);
            if (!isValid)
            {
                this.StatusMessage = "Please enter a valid password.";
            }

            return isValid;
        }

        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <param name="newPassword">The new password.</param>
        /// <returns>True if the password was reset successfully; otherwise, false.</returns>
        public async Task<bool> ResetPassword(string newPassword)
        {
            if (!this.ValidateNewPassword(newPassword))
            {
                return false;
            }

            if (!this.ValidatePasswordsMatch())
            {
                return false;
            }

            try
            {
                bool isReset = await this.forgotPassService.ResetPassword(this.Email, newPassword);

                if (isReset)
                {
                    this.StatusMessage = "Password reset successfully!";
                }
                else
                {
                    this.StatusMessage = "Failed to reset password. Please try again.";
                }

                return isReset;
            }
            catch (Exception ex)
            {
                this.StatusMessage = $"An error occurred: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}