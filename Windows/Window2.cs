using Google.Cloud.Firestore;
using Terminal.Gui;

namespace CpE261FinalProject
{
    public sealed class Window2 : AbstractWindow
    {
        private readonly FirestoreDb db = FirestoreManager.Instance.Database;
        private FirestoreChangeListener? _listener;

        private static readonly Lazy<Window2> lazyInstance = new(valueFactory: () => new Window2());

        public static Window2 Instance => lazyInstance.Value;
        private readonly Dictionary<string, string> currentChatroomParticipants = [];

        readonly List<View> chatInputViews = [chatroomName, buttonSend, chatBox, chatHistory];

        private Window2()
        {
            buttonSend.Clicked += OnButtonSendClicked;

            SessionHandler.CurrentChatroomChanged += OnChatroomChanged;
            SessionHandler.IsLoggedInChanged += OnLogInChanged;

            OnLogInChanged(IsLoggedIn: SessionHandler.IsLoggedIn);
        }

        public async void OnButtonSendClicked()
        {
            string? messageText = chatBox.Text.ToString();
            string? senderId = SessionHandler.UserId;
            string? chatroomId = SessionHandler.CurrentChatroomId;

            if (string.IsNullOrWhiteSpace(value: messageText) || senderId == null)
            {
                return;
            }
            chatBox.Text = string.Empty;

            Dictionary<string, object> message = new()
            {
                { "sender_id", senderId },
                { "text", messageText },
                { "timestamp", Timestamp.FromDateTime(DateTime.UtcNow) },
            };

            // ? Add Message logic is here
            await db.Collection(path: "Chatrooms")
                .Document(path: chatroomId)
                .Collection(path: "messages")
                .AddAsync(documentData: message);
        }

        public void OnLogInChanged(bool IsLoggedIn)
        {
            if (IsLoggedIn)
                OnChatroomChanged(SessionHandler.CurrentChatroomId);
            else
            {
                window.RemoveAll();
                window.Add(labelEmpty);
            }
        }

        private async void OnChatroomChanged(string? chatroom_id)
        {
            _listener?.StopAsync();
            _listener = null;

            chatHistory.SetSource(new List<string>());
            window.RemoveAll();
            Label loadingLabel = new("Loading chat...") { X = Pos.Center(), Y = Pos.Center() };
            window.Add(view: loadingLabel);

            if (SessionHandler.IsLoggedIn == false)
                return;

            if (string.IsNullOrEmpty(value: chatroom_id))
            {
                window.Add(view: labelEmptyChatroom);
                return;
            }

            await FetchAndCacheParticipants(chatroomId: chatroom_id);
            string _chatroomName = await FirebaseHelper.GetChatroomNameById(
                chatroom_id: SessionHandler.CurrentChatroomId! //! using `!` here
            );
            chatroomName.Text = _chatroomName;
            chatroomName.X = Pos.Center();
            StartListener(chatroom_id: chatroom_id);

            window.Remove(view: loadingLabel);
            window.Add(views: [.. chatInputViews]);
        }

        // TODO: Make this listen to cached
        private async Task FetchAndCacheParticipants(string chatroomId)
        {
            // Clear stale data
            currentChatroomParticipants.Clear();

            DocumentReference chatroomRef = db.Collection(path: "Chatrooms")
                .Document(path: chatroomId);
            DocumentSnapshot chatroomSnap = await chatroomRef.GetSnapshotAsync();

            if (!chatroomSnap.Exists)
                return;

            // if (chatroomSnap.TryGetValue("name", out string chatName))
            // {
            //     Application.MainLoop.Invoke(() =>
            //     {
            //         chatroomName.Text = chatName;
            //     });
            // }

            // Get the list of participant IDs from the `chatroomId`
            if (chatroomSnap.TryGetValue("participants", out List<string> participantIds))
            {
                foreach (string id in participantIds)
                {
                    DocumentSnapshot userSnap = await db.Collection(path: "Users")
                        .Document(path: id)
                        .GetSnapshotAsync();
                    if (userSnap.Exists && userSnap.TryGetValue("name", out string name))
                        currentChatroomParticipants[id] = name;
                }
            }
        }

        readonly Label labelEmptyChatroom = new()
        {
            Text = "You have not yet chosen a chatroom...",

            X = Pos.Center(),
            Y = Pos.Center(),
        };

        public void Show()
        {
            Application.Top.Add(views: [window]);
        }

        public void Hide()
        {
            Application.Top.Remove(view: window);
        }

        readonly Window window = new()
        {
            Title = "Main Chat",
            Height = Dim.Fill(),
            Width = Dim.Percent(n: 54),
            X = Pos.Percent(n: 23),
            Y = 0,
            ColorScheme = CustomColorScheme.Window,
        };

        static readonly Label chatroomName = new()
        {
            Text = "",
            X = Pos.Center(),
            Y = 0, // At the top
        };

        readonly Label labelEmpty = new()
        {
            Text = "Nothing to see here...",
            X = Pos.Center(),
            Y = Pos.Center(),
            ColorScheme = CustomColorScheme.LabelEmpty,
        };

        static readonly Button buttonSend = new()
        {
            Text = "Send",
            X = Pos.AnchorEnd(margin: 8),
            Y = Pos.AnchorEnd(margin: 1), // At the bottom
            ColorScheme = CustomColorScheme.Button,
        };

        static readonly TextField chatBox = new()
        {
            Width = Dim.Fill() - Dim.Width(view: buttonSend),
            X = 0,
            Y = Pos.AnchorEnd(margin: 1), // At the bottom
        };

        static readonly ListView chatHistory = new()
        {
            X = 0,
            Y = 5,

            Height = Dim.Sized(n: 20),
            Width = Dim.Fill(),
        };

        // TODO: Make this listen to cached
        private void StartListener(string chatroom_id)
        {
            CollectionReference messegesRef = db.Collection(path: "Chatrooms")
                .Document(path: chatroom_id)
                .Collection(path: "messages");

            Query query = messegesRef.OrderBy(fieldPath: "timestamp");

            _listener = query.Listen(callback: OnSnapshotReceived);
        }

        // TODO: Make this listen to cached
        private void OnSnapshotReceived(QuerySnapshot snapshot)
        {
            Application.MainLoop.Invoke(() =>
            {
                List<string> messages = [];
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (
                        document.Exists
                        && document.TryGetValue(path: "sender_id", out string senderId)
                        && document.TryGetValue(path: "text", out string text)
                    )
                    {
                        if (
                            !currentChatroomParticipants.TryGetValue(
                                key: senderId,
                                value: out string? senderName
                            )
                        )
                            senderName = "Unknown User"; // Fallback if user not in cache

                        string displayName =
                            (senderId == SessionHandler.UserId) ? $"{senderName} (me)" : senderName;

                        messages.Add(
                            item: $"{displayName}: {ProfanityChecker.CensorTextRobust(text: text)}"
                        );
                    }
                }

                while (messages.Count < chatHistory.Frame.Height)
                    messages.Insert(index: 0, item: ".");

                chatHistory.SetSource(source: messages);

                if (messages.Count > 0)
                {
                    chatHistory.ScrollDown(items: messages.Count - 1);
                    chatHistory.ScrollUp(items: chatHistory.Frame.Height - 2);
                }
            });
        }
    }
}
