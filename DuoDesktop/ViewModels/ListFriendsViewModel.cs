using DuolingoClassLibrary.Entities;
using Duo.Services;
using Duo.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Duo;
using System.Threading.Tasks;

namespace Duo.UI.ViewModels
{
    /// <summary>
    /// ViewModel for managing the list of friends
    /// </summary>
    public class ListFriendsViewModel : INotifyPropertyChanged
    {
        private readonly FriendsService friendsService;
        private List<User> friends = new List<User>();
        private int userId;

        /// <summary>
        /// Event that is triggered when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the list of friends
        /// </summary>
        public List<User> Friends
        {
            get => friends;
            set
            {
                if (friends != value)
                {
                    friends = value;
                    // Raise the PropertyChanged event whenever the value of Friends changes
                    OnPropertyChanged(nameof(Friends));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListFriendsViewModel"/> class
        /// </summary>
        /// <param name="friendsService">The friends service</param>
        public ListFriendsViewModel(FriendsService friendsService)
        {
            this.friendsService = friendsService ?? throw new ArgumentNullException(nameof(friendsService));
            
            // User ID will be set when LoadFriends is called with the current user
            this.userId = 0;
        }

        /// <summary>
        /// Loads friends from the service
        /// </summary>
        public async Task LoadFriendsAsync()
        {
            if (App.CurrentUser != null)
            {
                userId = App.CurrentUser.UserId;
                var loadedFriends = await friendsService.GetFriends(userId);
                Friends = loadedFriends;
            }
        }

        /// <summary>
        /// Sorts friends by name
        /// </summary>
        public async Task SortByNameAsync()
        {
            var sortedFriends = await friendsService.SortFriendsByName(userId);
            Friends = sortedFriends;
        }

        /// <summary>
        /// Sorts friends by date added
        /// </summary>
        public async Task SortByDateAddedAsync()
        {
            var sortedFriends = await friendsService.SortFriendsByDateAdded(userId);
            Friends = sortedFriends;
        }

        /// <summary>
        /// Sorts friends by online status
        /// </summary>
        public async Task SortByOnlineStatusAsync()
        {
            var sortedFriends = await friendsService.SortFriendsByOnlineStatus(userId);
            Friends = sortedFriends;
        }

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
