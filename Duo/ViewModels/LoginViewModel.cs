// <copyright file="LoginViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Services.Interfaces;

    /// <summary>
    /// ViewModel that handles the login logic.
    /// </summary>
    public class LoginViewModel
    {
        private readonly ILoginService loginService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
        /// </summary>
        /// <param name="loginService">The login service.</param>
        public LoginViewModel(ILoginService loginService)
        {
            this.loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        }

        /// <summary>
        /// Gets or sets the username entered by the user.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password entered by the user.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the login was successful.
        /// </summary>
        public bool LoginStatus { get; private set; }

        /// <summary>
        /// Gets the logged-in user after a successful login.
        /// </summary>
        public User? LoggedInUser { get; private set; }

        /// <summary>
        /// Attempts to log in with the provided credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task<bool> AttemptLogin(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                this.LoginStatus = false;
                return false;
            }

            this.Username = username;
            this.Password = password;

            try
            {
                // Try to get the user
                this.LoggedInUser = await this.loginService.GetUserByCredentials(this.Username, this.Password);
                this.LoginStatus = this.LoggedInUser != null;

                if (this.LoginStatus && this.LoggedInUser != null && App.UserService != null)
                {
                    App.CurrentUser = this.LoggedInUser;
                    await App.UserService.SetUser(App.CurrentUser.UserName);
                }

                return this.LoginStatus;
            }
            catch (Exception)
            {
                this.LoginStatus = false;
                return false;
            }
        }
    }
}