using Google.Cloud.Firestore;

namespace CpE261FinalProject
{
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
        public static event Action<string?>? UsernameChanged;
        public static event Action<string?>? NameChanged;
        public static event Action<string?>? UserIdChanged;
        public static event Action<string?>? CurrentChatroomChanged;
        public static event Action<bool>? IsLoggedInChanged;
        public static event Action<
            List<(string chatroom_id, string chatroom_name)>
        >? UserChatroomsChanged;

        // --- Public Properties ---

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

        // listens to the Database/Chatrooms and updates @Chatrooms property
        public static async Task StartChatroomsListener()
        {
            if (!IsLoggedIn)
                return;

            CollectionReference chatroomsRef = FirebaseHelper.db.Collection("Chatrooms");
            Query query = chatroomsRef.WhereArrayContains("participants", UserId);

            chatroomsListener = query.Listen(async snapshot =>
            {
                List<(string Id, string Name)> chatroomsList = [];

                List<string> ids = [.. chatroomsList.Select(x => x.Id)];

                if (!ids.Contains(SessionHandler.CurrentChatroomId!))
                    CurrentChatroomId = "";

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    string chatroom_name = await FirebaseHelper.GetChatroomNameById(doc.Id);

                    chatroomsList.Add((doc.Id, chatroom_name));
                }

                Chatrooms = chatroomsList;
            });
        }
    }
}
