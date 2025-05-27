// <copyright file="ManageSectionsViewModel.cs" company="YourCompany">
// Copyright (c) YourCompany. All rights reserved.
// </copyright>

namespace Duo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Duo.Commands;
    using Duo.ViewModels.Base;
    using DuoClassLibrary.Models.Quizzes;
    using DuoClassLibrary.Models.Sections;
    using DuoClassLibrary.Services;

    /// <summary>
    /// ViewModel for managing sections and their quizzes in the admin interface.
    /// </summary>
    internal class ManageSectionsViewModel : AdminBaseViewModel
    {
        private readonly ISectionService sectionService;
        private readonly IQuizService quizService;
        private Section? selectedSection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageSectionsViewModel"/> class.
        /// </summary>
        public ManageSectionsViewModel()
        {
            try
            {
                this.sectionService = (ISectionService)(App.ServiceProvider?.GetService(typeof(ISectionService)) ?? throw new InvalidOperationException("SectionService not found."));
                this.quizService = (IQuizService)(App.ServiceProvider?.GetService(typeof(IQuizService)) ?? throw new InvalidOperationException("QuizService not found."));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                this.RaiseErrorMessage("Initialization error", ex.Message);
            }

            this.DeleteSectionCommand = new RelayCommandWithParameter<Section>(section => _ = this.DeleteSection(section));
            _ = this.LoadSectionsAsync();
        }

        /// <summary>
        /// Gets the collection of sections.
        /// </summary>
        public ObservableCollection<Section> Sections { get; } = new ObservableCollection<Section>();

        /// <summary>
        /// Gets the collection of quizzes in the selected section.
        /// </summary>
        public ObservableCollection<Quiz> SectionQuizes { get; } = new ObservableCollection<Quiz>();

        /// <summary>
        /// Gets or sets the currently selected section.
        /// </summary>
        public Section? SelectedSection
        {
            get => this.selectedSection;
            set
            {
                this.selectedSection = value;
                _ = this.UpdateSectionQuizes(this.SelectedSection);
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the command to delete a section.
        /// </summary>
        public ICommand DeleteSectionCommand { get; }

        /// <summary>
        /// Loads all sections asynchronously and updates the collection.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadSectionsAsync()
        {
            try
            {
                var sections = await this.sectionService.GetAllSections();
                this.Sections.Clear();

                foreach (var section in sections)
                {
                    this.Sections.Add(section);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadSectionsAsync error: {ex.Message}");
                this.RaiseErrorMessage("Failed to load sections", ex.Message);
            }
        }

        /// <summary>
        /// Updates the quizzes for the selected section.
        /// </summary>
        /// <param name="selectedSection">The selected section.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UpdateSectionQuizes(Section? selectedSection)
        {
            try
            {
                Debug.WriteLine("Updating section quizzes...");
                this.SectionQuizes.Clear();

                if (selectedSection == null)
                {
                    Debug.WriteLine("No section selected. Skipping update.");
                    return;
                }

                var quizzes = await this.quizService.GetAllQuizzesFromSection(selectedSection.Id);
                foreach (var quiz in quizzes)
                {
                    this.SectionQuizes.Add(quiz);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateSectionQuizes error: {ex.Message}");
                this.RaiseErrorMessage("Failed to load quizzes for section", ex.Message);
            }
        }

        /// <summary>
        /// Deletes a section and updates the collection.
        /// </summary>
        /// <param name="sectionToBeDeleted">The section to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DeleteSection(Section sectionToBeDeleted)
        {
            try
            {
                Debug.WriteLine("Deleting section...");

                if (sectionToBeDeleted == this.SelectedSection)
                {
                    this.SelectedSection = null;
                }

                await this.sectionService.DeleteSection(sectionToBeDeleted.Id);
                this.Sections.Remove(sectionToBeDeleted);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteSection error: {ex.Message}");
                this.RaiseErrorMessage("Failed to delete section", ex.Message);
            }
        }
    }
}