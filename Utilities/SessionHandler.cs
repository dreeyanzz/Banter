using Google.Cloud.Firestore;

namespace Banter.Utilities
{
    /// <summary>
    /// Manages the user's session, including login state, user information, and chatroom data.
    /// </summary>
    public static class SessionHandler
    {
        private static FirestoreChangeListener? chatroomsListener;

        // --- Private Backing Fields ---
        private static string? _username;
        private static string? _name;
        private static string? _user_id;
        private static string? _current_chatroom_id;
        private static bool _isLoggedIn = false;
        private static List<(string chatroom_id, string chatroom_name)> _userChatrooms = [];

        // --- Public Events ---
        /// <summary>
        /// Occurs when the username changes.
        /// </summary>
        public static event Action<string?>? UsernameChanged;

        /// <summary>
        /// Occurs when the name changes.
        /// </summary>
        public static event Action<string?>? NameChanged;

        /// <summary>
        /// Occurs when the user ID changes.
        /// </summary>
        public static event Action<string?>? UserIdChanged;

        /// <summary>
        /// Occurs when the current chatroom changes.
        /// </summary>
        public static event Action<string?>? CurrentChatroomChanged;

        /// <summary>
        /// Occurs when the login state changes.
        /// </summary>
        public static event Action<bool>? IsLoggedInChanged;

        /// <summary>
        /// Occurs when the user's chatrooms change.
        /// </summary>
        public static event Action<
            List<(string chatroom_id, string chatroom_name)>
        >? UserChatroomsChanged;

        // --- Public Properties ---

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
            get => _user_id;
            set
            {
                if (_user_id != value)
                {
                    _user_id = value;
                    UserIdChanged?.Invoke(_user_id);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ID of the currently active chatroom.
        /// </summary>
        public static string? CurrentChatroomId
        {
            get => _current_chatroom_id;
            set
            {
                if (IsLoggedIn == false)
                    return;

                if (_current_chatroom_id != value)
                {
                    _current_chatroom_id = value;
                    CurrentChatroomChanged?.Invoke(_current_chatroom_id);
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
                if (_userChatrooms != value)
                {
                    _userChatrooms = value;
                    UserChatroomsChanged?.Invoke(_userChatrooms);
                }
            }
        }

        /// <summary>
        /// Clears the current session data and logs the user out.
        /// </summary>
        public static async Task ClearSession()
        {
            if (chatroomsListener != null)
                await chatroomsListener.StopAsync();

            Username = null;
            Name = null;
            UserId = null;
            CurrentChatroomId = null;
            IsLoggedIn = false;
            Chatrooms = [];
        }

        /// <summary>
        /// Starts a listener for changes to the user's chatrooms in the database.
        /// </summary>
        public static async Task StartChatroomsListener()
        {
            if (!IsLoggedIn)
                return;

            CollectionReference chatroomsRef = FirebaseHelper.db.Collection("Chatrooms");
            Query query = chatroomsRef.WhereArrayContains("participants", UserId);

            chatroomsListener = query.Listen(async snapshot =>
            {
                List<(string Id, string Name)> chatroomsList = [];

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    string chatroom_name = await FirebaseHelper.GetChatroomNameById(doc.Id);

                    chatroomsList.Add((doc.Id, chatroom_name));
                }

                List<string> chatroomIds = [.. chatroomsList.Select(x => x.Id)]; // no value

                if (!chatroomIds.Contains(SessionHandler.CurrentChatroomId!)) //! quite incomprehensive
                    CurrentChatroomId = "";

                Chatrooms = chatroomsList;
            });
        }
    }
}
