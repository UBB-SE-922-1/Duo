// <copyright file="SignUpViewModel.cs" company="YourCompany">
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
    using DuoClassLibrary.Services;

    /// <summary>
    /// ViewModel for handling sign-up operations and validation.
    /// </summary>
    public class SignUpViewModel : INotifyPropertyChanged
    {
        private readonly SignUpService signUpService;
        private readonly SignUpValidator validator;
        private User newUser = new User();
        private string confirmPassword = string.Empty;
        private string passwordStrength = string.Empty;
        private string usernameValidationMessage = string.Empty;
        private string passwordValidationMessage = string.Empty;
        private string confirmPasswordValidationMessage = string.Empty;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignUpViewModel"/> class.
        /// </summary>
        /// <param name="signUpService">The sign-up service.</param>
        public SignUpViewModel(SignUpService signUpService)
        {
            this.signUpService = signUpService ?? throw new ArgumentNullException(nameof(signUpService));
            this.validator = new SignUpValidator();
        }

        /// <summary>
        /// Gets or sets the new user being created.
        /// </summary>
        public User NewUser
        {
            get => this.newUser;
            set
            {
                this.newUser = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the password confirmation.
        /// </summary>
        public string ConfirmPassword
        {
            get => this.confirmPassword;
            set
            {
                this.confirmPassword = value;
                this.ValidatePasswordMatch();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the password strength indicator.
        /// </summary>
        public string PasswordStrength
        {
            get => this.passwordStrength;
            set
            {
                this.passwordStrength = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the username validation message.
        /// </summary>
        public string UsernameValidationMessage
        {
            get => this.usernameValidationMessage;
            set
            {
                this.usernameValidationMessage = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the password validation message.
        /// </summary>
        public string PasswordValidationMessage
        {
            get => this.passwordValidationMessage;
            set
            {
                this.passwordValidationMessage = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the confirm password validation message.
        /// </summary>
        public string ConfirmPasswordValidationMessage
        {
            get => this.confirmPasswordValidationMessage;
            set
            {
                this.confirmPasswordValidationMessage = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Checks if a username is already taken.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <returns>True if the username is taken; otherwise, false.</returns>
        public async Task<bool> IsUsernameTaken(string username)
        {
            try
            {
                return await this.signUpService.IsUsernameTaken(username);
            }
            catch (Exception checkUsernameException)
            {
                Console.WriteLine($"Error checking username: {checkUsernameException.Message}");
                return true;
            }
        }

        /// <summary>
        /// Creates a new user with the provided information.
        /// </summary>
        /// <param name="user">The user information.</param>
        /// <returns>True if the user was created successfully; otherwise, false.</returns>
        public async Task<bool> CreateNewUser(User user)
        {
            try
            {
                user.DateJoined = DateTime.Now;
                return await this.signUpService.RegisterUser(user);
            }
            catch (Exception createUserException)
            {
                Console.WriteLine($"Error creating user: {createUserException.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validates if the username follows the required format.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <returns>True if the username is valid; otherwise, false.</returns>
        public bool ValidateUsername(string username)
        {
            bool isValid = SignUpValidator.IsValidUsername(username);
            this.UsernameValidationMessage = isValid ? string.Empty : "Username must be 5-20 characters and contain only letters, digits, or underscores.";
            return isValid;
        }

        /// <summary>
        /// Updates the password strength indicator based on the provided password.
        /// </summary>
        /// <param name="password">The password to evaluate.</param>
        public void UpdatePasswordStrength(string password)
        {
            this.PasswordStrength = SignUpValidator.GetPasswordStrength(password);

            if (this.PasswordStrength == "Weak")
            {
                this.PasswordValidationMessage = "Password must include uppercase, digit, and special character.";
            }
            else
            {
                this.PasswordValidationMessage = string.Empty;
            }
        }

        /// <summary>
        /// Validates if the password is strong enough.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>True if the password is medium or strong; otherwise, false.</returns>
        public bool ValidatePasswordStrength(string password)
        {
            string strength = SignUpValidator.GetPasswordStrength(password);
            return strength != "Weak";
        }

        /// <summary>
        /// Validates if the passwords match.
        /// </summary>
        /// <returns>True if the passwords match; otherwise, false.</returns>
        public bool ValidatePasswordMatch()
        {
            bool match = SignUpValidator.DoPasswordsMatch(this.NewUser.Password, this.ConfirmPassword);
            this.ConfirmPasswordValidationMessage = match ? string.Empty : "Passwords do not match.";
            return match;
        }

        /// <summary>
        /// Validates all sign-up information.
        /// </summary>
        /// <returns>True if all validations pass; otherwise, false.</returns>
        public bool ValidateAll()
        {
            bool usernameValid = this.ValidateUsername(this.NewUser.UserName);
            bool passwordStrengthValid = this.ValidatePasswordStrength(this.NewUser.Password);
            bool passwordsMatch = this.ValidatePasswordMatch();

            return usernameValid && passwordStrengthValid && passwordsMatch;
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