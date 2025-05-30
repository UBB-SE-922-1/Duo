﻿using DuoClassLibrary.Models.Exercises;
using System.Collections.Generic;

namespace WebServerTest.Models
{
    public class CreateQuizViewModel
    {
        public List<int> SelectedExerciseIds { get; set; } = new();
        public IEnumerable<Exercise> AvailableExercises { get; set; } = new List<Exercise>();
    }
}
