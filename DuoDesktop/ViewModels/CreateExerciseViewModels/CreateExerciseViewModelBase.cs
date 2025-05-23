using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuoClassLibrary.Models;
using DuoClassLibrary.Models.Exercises;
using DuoDesktop.ViewModels.Base;

namespace DuoDesktop.ViewModels.ExerciseViewModels
{
    internal abstract class CreateExerciseViewModelBase : ViewModelBase
    {
        public abstract Exercise CreateExercise(string question, Difficulty difficulty);
    }
}
