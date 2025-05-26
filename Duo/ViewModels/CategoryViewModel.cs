// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryViewModel.cs" company="YourCompanyName">
//   Copyright (c) YourCompanyName. All rights reserved.
// </copyright>
// <summary>
//   ViewModel for managing categories.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.Services;
    using DuoClassLibrary.Models;
    using DuoClassLibrary.Services.Interfaces;
    using Microsoft.UI.Xaml;

    /// <summary>
    /// ViewModel for managing categories.
    /// </summary>
    public class CategoryViewModel : INotifyPropertyChanged
    {
        private readonly ICategoryService categoryService;
        private string categoryName = string.Empty;
        private List<Category> categories = new List<Category>();
        private bool isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryViewModel"/> class.
        /// </summary>
        /// <param name="categoryService">The category service to use for data operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="categoryService"/> is null.</exception>
        public CategoryViewModel(ICategoryService categoryService)
        {
            this.categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _ = this.LoadCategoriesAsync();
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the list of categories.
        /// </summary>
        public List<Category> Categories
        {
            get => this.categories;
            set
            {
                if (this.categories != value)
                {
                    this.categories = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the data is loading.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set
            {
                if (this.isLoading != value)
                {
                    this.isLoading = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Asynchronously loads the list of categories.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadCategoriesAsync()
        {
            try
            {
                this.IsLoading = true;
                this.Categories = await this.categoryService.GetAllCategories();
            }
            catch (Exception ex)
            {
                // Log the error or show a message to the user
                Console.WriteLine($"Error loading categories: {ex.Message}");
                this.Categories = new List<Category>();
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Gets the names of all categories.
        /// </summary>
        /// <returns>A list of category names.</returns>
        public List<string> GetCategoryNames()
        {
            if (this.Categories == null || this.Categories.Count == 0)
            {
                return new List<string>();
            }

            var catNames = this.Categories.Select(c => c.Name).ToList();
            return catNames;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
