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
        readonly List<string> message_ids = [];
        readonly List<string> messages = [];

        readonly List<View> chatInputViews;

        private Window2()
        {
            chatInputViews =
            [
                chatroomName,
                viewPinnedMessagesButton,
                searchChatLabel,
                clearSearchChatButton,
                searchChatTextField,
                chatHistory,
                buttonSend,
                chatBox,
            ];

            Application.MainLoop.Invoke(action: () => chatHistory.SetSource(source: messages));

            SessionHandler.CurrentChatroomChanged += OnChatroomChanged;
            SessionHandler.IsLoggedInChanged += OnLogInChanged;

            buttonSend.Clicked += OnButtonSendClicked;
            searchChatTextField.TextChanged += (_) => OnSearchChatTextChanged();

            OnLogInChanged(IsLoggedIn: SessionHandler.IsLoggedIn);
        }

        private void OnSearchChatTextChanged()
        {
            string textToSearch = searchChatTextField.Text.ToString() ?? "";

            List<string> filtered;

            if (!string.IsNullOrWhiteSpace(value: textToSearch))
            {
                searchIndicatorLabel.Text = $"Searching for: {textToSearch}";

                filtered = [.. messages.Where(entry => entry.Contains(value: textToSearch))];

                while (filtered.Count < chatHistory.Frame.Height)
                    filtered.Insert(0, ".");

                Application.MainLoop.Invoke(() => chatHistory.SetSource(source: filtered));
            }
            else
            {
                searchIndicatorLabel.Text = string.Empty;

                while (messages.Count < chatHistory.Frame.Height)
                    messages.Insert(index: 0, item: ".");

                Application.MainLoop.Invoke(action: () => chatHistory.SetSource(source: messages));
            }

            Application.MainLoop.Invoke(action: ScrollToLatestChat);
        }

        private async void OnButtonSendClicked()
        {
            string? messageText = chatBox.Text.ToString();
            string? senderId = SessionHandler.UserId;
            string? chatroomId = SessionHandler.CurrentChatroomId;

            if (string.IsNullOrWhiteSpace(value: messageText) || senderId == null)
                return;
            chatBox.Text = string.Empty;

            Dictionary<string, object> message = new()
            {
                { "sender_id", senderId },
                { "text", messageText },
                { "timestamp", Timestamp.FromDateTime(dateTime: DateTime.UtcNow) },
            };

            // ? Add Message logic is here
            //TODO: your forgot to update last chat dawg
            await db.Collection(path: "Chatrooms")
                .Document(path: chatroomId)
                .Collection(path: "messages")
                .AddAsync(documentData: message);
        }

        private void OnLogInChanged(bool IsLoggedIn)
        {
            if (!IsLoggedIn)
            {
                window.RemoveAll();
                window.Add(view: labelEmpty);
                return;
            }

            OnChatroomChanged(chatroom_id: SessionHandler.CurrentChatroomId);
        }

        private async void OnChatroomChanged(string? chatroom_id)
        {
            _listener?.StopAsync();
            _listener = null;

            chatHistory.SetSource(source: new List<string>());
            messages.Clear();
            chatHistory.SetSource(source: messages);
            while (messages.Count < chatHistory.Frame.Height)
                messages.Insert(index: 0, item: ".");

            window.RemoveAll();

            Label loadingLabel = new(text: "Loading chat...")
            {
                X = Pos.Center(),
                Y = Pos.Center(),
            };
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

            Application.MainLoop.Invoke(action: () =>
            {
                chatroomName.Text = _chatroomName;
                chatroomName.X = Pos.Center();
            });

            StartListener(chatroom_id: chatroom_id);

            Application.MainLoop.Invoke(action: () =>
            {
                window.Remove(view: loadingLabel);
                window.Add(views: [.. chatInputViews]);
            });
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

            if (
                chatroomSnap.TryGetValue(
                    path: "participants",
                    value: out List<string> participantIds
                )
            )
            {
                foreach (string id in participantIds)
                {
                    DocumentSnapshot userSnap = await db.Collection(path: "Users")
                        .Document(path: id)
                        .GetSnapshotAsync();
                    if (
                        userSnap.Exists
                        && userSnap.TryGetValue(path: "name", value: out string name)
                    )
                        currentChatroomParticipants[id] = name;
                }
            }
        }

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

        readonly Label labelEmptyChatroom = new()
        {
            Text = "You have not yet chosen a chatroom...",

            X = Pos.Center(),
            Y = Pos.Center(),
        };

        readonly Label labelEmptyChat = new()
        {
            Text = "No chats yet, better strike a conversation first!",

            X = Pos.Center(),
            Y = Pos.Center(),
        };

        readonly Label chatroomName = new()
        {
            Text = "",

            X = Pos.Center(),
            Y = 1, // At the top
        };

        readonly Label labelEmpty = new()
        {
            Text = "Nothing to see here...",

            X = Pos.Center(),
            Y = Pos.Center(),

            ColorScheme = CustomColorScheme.LabelEmpty,
        };

        readonly Button viewPinnedMessagesButton = new()
        {
            Text = "View pinned messages",

            X = Pos.Center(),
            Y = Pos.At(n: 3),
        };

        static readonly Label searchChatLabel = new()
        {
            Text = "Search chat:",

            X = 0,
            Y = Pos.At(n: 5),
        };

        readonly TextField searchChatTextField = new()
        {
            X = Pos.Right(view: searchChatLabel) + Pos.At(n: 1),
            Y = Pos.Y(view: searchChatLabel),

            Width = Dim.Fill() - Dim.Width(view: clearSearchChatButton),
        };

        static readonly Button clearSearchChatButton = new()
        {
            Text = "Clear",

            X = Pos.AnchorEnd() - Pos.At(n: "Clear".Length + 4),
            Y = Pos.Y(view: searchChatLabel),

            ColorScheme = CustomColorScheme.Button,
        };

        readonly Label searchIndicatorLabel = new()
        {
            Text = "",

            X = 0,
            Y = Pos.At(n: 6),
        };

        readonly ListView chatHistory = new()
        {
            X = 0,
            Y = Pos.At(n: 7),

            Height = Dim.Sized(n: 20),
            Width = Dim.Fill(),
        };

        static readonly Button buttonSend = new()
        {
            Text = "Send",

            X = Pos.AnchorEnd(margin: 8),
            Y = Pos.AnchorEnd() - Pos.At(n: 1), // At the bottom

            ColorScheme = CustomColorScheme.Button,
        };

        readonly TextField chatBox = new()
        {
            Width = Dim.Fill() - Dim.Width(view: buttonSend),

            X = 0,
            Y = Pos.AnchorEnd(margin: 1), // At the bottom
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
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                bool exists = message_ids.Contains(item: document.Id);
                if (exists)
                    continue;

                if (
                    !document.Exists
                    || !document.TryGetValue(path: "sender_id", value: out string senderId)
                    || !document.TryGetValue(path: "text", value: out string message)
                )
                    continue;

                if (
                    !currentChatroomParticipants.TryGetValue(
                        key: senderId,
                        value: out string? senderName
                    )
                )
                    senderName = "Unknown User"; // Fallback if user not in cache

                string displayName =
                    (senderId == SessionHandler.UserId) ? $"{senderName} (me)" : senderName;
                string chatEntry =
                    $"{displayName}: {ProfanityChecker.CensorTextRobust(text: message)}";

                message_ids.Add(item: document.Id);
                messages.Add(item: chatEntry);
            }

            Application.MainLoop.Invoke(action: ScrollToLatestChat);
        }

        private void ScrollToLatestChat()
        {
            chatHistory.ScrollDown(items: messages.Count - 1);
            chatHistory.ScrollUp(items: chatHistory.Frame.Height - 1);
        }
    }
}
