// <copyright file="IModuleViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System.Threading.Tasks;
    using System.Windows.Input;
    using DuoClassLibrary.Models;

    /// <summary>
    /// Interface for the view model that handles module-related operations and state.
    /// </summary>
    public interface IModuleViewModel : IBaseViewModel
    {
        /// <summary>
        /// Gets the current module being displayed or worked on.
        /// </summary>
        Module CurrentModule { get; }

        /// <summary>
        /// Gets a value indicating whether the current module has been completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Initializes the module view model asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InitializeAsync();

        /// <summary>
        /// Gets the command that triggers the completion of the current module.
        /// </summary>
        ICommand CompleteModuleCommand { get; }

        /// <summary>
        /// Gets or sets the command for handling actions related to the module's image click.
        /// </summary>
        ICommand ModuleImageClickCommand { get; set; }

        /// <summary>
        /// Gets the formatted time spent on the current module.
        /// </summary>
        string TimeSpent { get; }

        /// <summary>
        /// Gets the current coin balance related to the module.
        /// </summary>
        int CoinBalance { get; }

        /// <summary>
        /// Handles the image click event for the current module, performing necessary actions.
        /// </summary>
        /// <param name="obj">The event parameter or context object.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task HandleModuleImageClick(object? obj);

        /// <summary>
        /// Executes the module image click logic, triggering associated actions and UI updates.
        /// </summary>
        /// <param name="obj">The event parameter or context object.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteModuleImageClick(object? obj);
    }
}