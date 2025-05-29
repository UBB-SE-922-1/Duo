// <copyright file="Timer.xaml.cs" company="DuoISS">
// Copyright (c) DuoISS. All rights reserved.
// </copyright>

namespace Duo.Views.Components
{
    using System;
    using System.Diagnostics;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// Timer component for displaying elapsed time in various app features.
    /// </summary>
    public sealed partial class Timer : UserControl
    {
        private readonly DispatcherTimer timer;
        private readonly Stopwatch stopwatch;
        private bool isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="Timer"/> class.
        /// </summary>
        public Timer()
        {
            this.InitializeComponent();

            this.stopwatch = new Stopwatch();
            this.timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100),
            };
            this.timer.Tick += this.Timer_Tick;
            this.isRunning = false;
        }

        /// <summary>
        /// Occurs on each timer update with the current elapsed time.
        /// </summary>
        public event EventHandler<TimeSpan>? TimerTick;

        /// <summary>
        /// Starts the timer if not already running.
        /// </summary>
        public void Start()
        {
            if (!this.isRunning)
            {
                this.stopwatch.Start();
                this.timer.Start();
                this.isRunning = true;
            }
        }

        /// <summary>
        /// Pauses the timer if currently running.
        /// </summary>
        public void Stop()
        {
            if (this.isRunning)
            {
                this.stopwatch.Stop();
                this.timer.Stop();
                this.isRunning = false;
            }
        }

        /// <summary>
        /// Resets the timer to zero.
        /// </summary>
        public void Reset()
        {
            this.Stop();
            this.stopwatch.Reset();
            this.UpdateTime();
        }

        /// <summary>
        /// Gets the current elapsed time.
        /// </summary>
        /// <returns>The elapsed <see cref="TimeSpan"/>.</returns>
        public TimeSpan GetElapsedTime()
        {
            return this.stopwatch.Elapsed;
        }

        private void Timer_Tick(object? sender, object e)
        {
            this.UpdateTime();
            this.TimerTick?.Invoke(this, this.stopwatch.Elapsed);
        }

        private void UpdateTime()
        {
            TimeSpan elapsed = this.stopwatch.Elapsed;
            string formattedTime = string.Format("{0:00}:{1:00}", elapsed.Minutes, elapsed.Seconds);
            this.TimerDisplay.Text = formattedTime;
        }
    }
}