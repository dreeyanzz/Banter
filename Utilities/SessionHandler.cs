using Google.Cloud.Firestore;

namespace Banter.Utilities
{
    /// <summary>
    /// Manages the user's session, including login state, user information, and chatroom data.
    /// </summary>
    public static class SessionHandler
    {
        private static FirestoreChangeListener? chatroomsListener;

        #region Private Backing Fields

        private static string? _username;
        private static string? _name;
        private static string? _userId;
        private static string? _currentChatroomId;
        private static bool _isLoggedIn = false;
        private static List<(string ChatroomId, string ChatroomName)> _userChatrooms = [];

        #endregion

        #region Public Events

        public static event Action<string?>? UsernameChanged;
        public static event Action<string?>? NameChanged;
        public static event Action<string?>? UserIdChanged;
        public static event Action<string?>? CurrentChatroomChanged;
        public static event Action<bool>? IsLoggedInChanged;
        public static event Action<
            List<(string ChatroomId, string ChatroomName)>
        >? UserChatroomsChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the current user's username.
        /// </summary>
        public static string? Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    UsernameChanged?.Invoke(_username);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current user's name.
        /// </summary>
        public static string? Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NameChanged?.Invoke(_name);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current user's ID.
        /// </summary>
        public static string? UserId
        {
            get => _userId;
            set
            {
                if (_userId != value)
                {
                    _userId = value;
                    UserIdChanged?.Invoke(_userId);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ID of the currently active chatroom.
        /// </summary>
        public static string? CurrentChatroomId
        {
            get => _currentChatroomId;
            set
            {
                // Removed the "if (!IsLoggedIn)" check here.
                // We want to be able to clear this value (set to null) even during logout.
                if (_currentChatroomId != value)
                {
                    _currentChatroomId = value;
                    CurrentChatroomChanged?.Invoke(_currentChatroomId);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is logged in.
        /// </summary>
        public static bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                if (_isLoggedIn != value)
                {
                    _isLoggedIn = value;
                    IsLoggedInChanged?.Invoke(_isLoggedIn);
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of chatrooms the user is a member of.
        /// </summary>
        public static List<(string chatroom_id, string chatroom_name)> Chatrooms
        {
            get => _userChatrooms;
            set
            {
                // Note: Reference equality check is fine here if we always assign a NEW list instance.
                if (_userChatrooms != value)
                {
                    _userChatrooms = value;
                    UserChatroomsChanged?.Invoke(_userChatrooms);
                }
            }
        }

        #endregion

        #region Session Lifecycle

        /// <summary>
        /// Clears the current session data and logs the user out.
        /// </summary>
        public static async Task ClearSession()
        {
            if (chatroomsListener != null)
                await chatroomsListener.StopAsync();

            // Reset properties (triggers events for UI cleanup)
            CurrentChatroomId = null;
            Chatrooms = [];
            Username = null;
            Name = null;
            UserId = null;

            // Set IsLoggedIn last to ensure UI knows we are fully done
            IsLoggedIn = false;
        }

        /// <summary>
        /// Starts a listener for changes to the user's chatrooms in the database.
        /// </summary>
        public static async Task StartChatroomsListener()
        {
            if (!IsLoggedIn || string.IsNullOrEmpty(UserId))
                return;

            CollectionReference chatroomsRef = FirebaseHelper.db.Collection("Chatrooms");
            Query query = chatroomsRef.WhereArrayContains("participants", UserId);

            chatroomsListener = query.Listen(async snapshot =>
            {
                // OPTIMIZATION: Use Task.WhenAll to fetch names in parallel
                // instead of waiting for them one by one in a foreach loop.
                var chatroomTasks = snapshot.Documents.Select(async doc =>
                {
                    string name = await FirebaseHelper.GetChatroomNameById(doc.Id);
                    return (ChatroomId: doc.Id, ChatroomName: name);
                });

                var results = await Task.WhenAll(chatroomTasks);
                var chatroomsList = results.ToList();

                // Validation: Check if the user was kicked out of the current chatroom
                // We use ?. operator to avoid crashing if CurrentChatroomId is null
                if (
                    !string.IsNullOrEmpty(CurrentChatroomId)
                    && !chatroomsList.Any(x => x.ChatroomId == CurrentChatroomId)
                )
                {
                    CurrentChatroomId = null; // Kick user to dashboard/main menu
                }

                Chatrooms = chatroomsList;
            });
        }

        #endregion
    }
}
