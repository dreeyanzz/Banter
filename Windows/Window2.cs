using Google.Cloud.Firestore;
using Terminal.Gui;

namespace CpE261FinalProject
{
    public sealed class Window2 : AbstractWindow
    {
        private readonly FirestoreDb db = FirestoreManager.Instance.Database;
        private FirestoreChangeListener? _listener;
        private FirestoreChangeListener? chatroomListener;
        private static readonly Lazy<Window2> lazyInstance = new(valueFactory: () => new Window2());

        public static Window2 Instance => lazyInstance.Value;
        private readonly Dictionary<string, string> currentChatroomParticipants = [];
        private readonly List<string> message_ids = [];
        private readonly List<string> messages = [];
        private List<string> currentPinnedIds = [];
        private int numFill = 0;

        private readonly List<View> chatInputViews;

        private Window2()
        {
            chatInputViews =
            [
                chatroomName,
                viewPinnedMessagesButton,
                searchChatLabel,
                searchChatTextField,
                clearSearchChatButton,
                searchIndicatorLabel,
                chatHistory,
                buttonSend,
                chatBox,
            ];

            Application.MainLoop.Invoke(action: () => chatHistory.SetSource(source: messages));

            SessionHandler.CurrentChatroomChanged += OnChatroomChanged;
            SessionHandler.IsLoggedInChanged += OnLogInChanged;

            buttonSend.Clicked += OnButtonSendClicked;
            searchChatTextField.TextChanged += (_) => OnSearchChatTextChanged();
            clearSearchChatButton.Clicked += () => searchChatTextField.Text = string.Empty;
            viewPinnedMessagesButton.Clicked += ViewPinnedMessagesWindow.Instance.Show;

            chatHistory.OpenSelectedItem += async (_) =>
            {
                int selectedIndex = chatHistory.SelectedItem - numFill;

                if (selectedIndex < 0)
                    return;

                string chat_id = message_ids[selectedIndex];

                await FirebaseHelper.PinChatroomMessage(
                    chatroom_id: SessionHandler.CurrentChatroomId!,
                    message_id: chat_id
                ); //! USING `!` here!
            };

            window.Enter += (_) =>
            {
                buttonSend.IsDefault = true;
                Application.MainLoop.Invoke(action: () => chatBox.SetFocus());
            };
            window.Leave += (_) => buttonSend.IsDefault = false;

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

                bool needsFill = filtered.Count < chatHistory.Frame.Height;
                numFill = Math.Max(val1: 0, val2: chatHistory.Frame.Height - filtered.Count);
                IEnumerable<string> filler = Enumerable.Repeat(element: ".", count: numFill);
                Application.MainLoop.Invoke(action: () =>
                    chatHistory.SetSource(source: needsFill ? [.. filler, .. filtered] : filtered)
                );
            }
            else
            {
                searchIndicatorLabel.Text = string.Empty;

                bool needsFill = messages.Count < chatHistory.Frame.Height;
                numFill = Math.Max(0, chatHistory.Frame.Height - messages.Count);
                IEnumerable<string> filler = Enumerable.Repeat(".", numFill);
                Application.MainLoop.Invoke(action: () =>
                    chatHistory.SetSource(source: needsFill ? [.. filler, .. messages] : messages)
                );
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
            ViewPinnedMessagesWindow.Instance.Hide();

            _listener?.StopAsync();
            _listener = null;
            chatroomListener?.StopAsync();
            chatroomListener = null;

            chatHistory.SetSource(source: new List<string>());
            messages.Clear();
            message_ids.Clear();

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
            await StartChatroomListener(chatroom_id: chatroom_id);

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

        private readonly Window window = new()
        {
            Title = "Main Chat",

            Height = Dim.Fill(),
            Width = Dim.Percent(n: 54),

            X = Pos.Percent(n: 23),
            Y = 0,

            ColorScheme = CustomColorScheme.Window,
        };

        private readonly Label labelEmptyChatroom = new()
        {
            Text = "You have not yet chosen a chatroom...",

            X = Pos.Center(),
            Y = Pos.Center(),
        };

        private readonly Label labelEmptyChat = new()
        {
            Text = "No chats yet, better strike a conversation first!",

            X = Pos.Center(),
            Y = Pos.Center(),
        };

        private readonly Label chatroomName = new()
        {
            Text = "",

            X = Pos.Center(),
            Y = Pos.At(n: 1), // At the top
        };

        private readonly Label labelEmpty = new()
        {
            Text = "Nothing to see here...",

            X = Pos.Center(),
            Y = Pos.Center(),

            ColorScheme = CustomColorScheme.LabelEmpty,
        };

        private readonly Button viewPinnedMessagesButton = new()
        {
            Text = "View pinned messages",

            X = Pos.Center(),
            Y = Pos.At(n: 3),

            HotKeySpecifier = (Rune)0xffff,
        };

        private static readonly Label searchChatLabel = new()
        {
            Text = "Search chat:",

            X = 0,
            Y = Pos.At(n: 5),
        };

        private readonly TextField searchChatTextField = new()
        {
            X = Pos.Right(view: searchChatLabel) + Pos.At(n: 1),
            Y = Pos.Y(view: searchChatLabel),

            Width = Dim.Fill() - Dim.Width(view: clearSearchChatButton),
        };

        private static readonly Button clearSearchChatButton = new()
        {
            Text = "Clear",

            X = Pos.AnchorEnd() - Pos.At(n: "Clear".Length + 4),
            Y = Pos.Y(view: searchChatLabel),

            HotKeySpecifier = (Rune)0xffff,
        };

        private readonly Label searchIndicatorLabel = new()
        {
            Text = "",

            X = 0,
            Y = Pos.At(n: 6),
        };

        private readonly ListView chatHistory = new()
        {
            X = 0,
            Y = Pos.At(n: 7),

            Height = Dim.Sized(n: 20),
            Width = Dim.Fill(),
        };

        private readonly TextField chatBox = new()
        {
            Width = Dim.Fill() - Dim.Width(view: buttonSend),

            X = 0,
            Y = Pos.AnchorEnd() - Pos.At(n: 1), // At the bottom
        };

        private static readonly Button buttonSend = new()
        {
            Text = "Send",

            X = Pos.AnchorEnd() - Pos.At(n: "Send".Length + 6),
            Y = Pos.AnchorEnd() - Pos.At(n: 1), // At the bottom

            IsDefault = true,
            HotKeySpecifier = (Rune)0xffff,
        };

        // TODO: Make this listen to cached
        private void StartListener(string chatroom_id)
        {
            CollectionReference messegesRef = db.Collection(path: "Chatrooms")
                .Document(path: chatroom_id)
                .Collection(path: "messages");

            Query query = messegesRef.OrderBy(fieldPath: "timestamp");

            _listener = query.Listen(callback: (snapshot) => _ = OnSnapshotReceived(snapshot));
        }

        // TODO: Make this listen to cached
        private async Task OnSnapshotReceived(QuerySnapshot snapshot)
        {
            foreach (DocumentChange? change in snapshot.Changes)
            {
                string docId = change.Document.Id;

                // Handle removals
                if (change.ChangeType == DocumentChange.Type.Removed)
                {
                    int index = message_ids.IndexOf(item: docId);
                    if (index >= 0)
                    {
                        message_ids.RemoveAt(index: index);
                        messages.RemoveAt(index: index);
                    }
                    continue;
                }

                // Handle added or modified
                DocumentSnapshot doc = change.Document;

                if (
                    !doc.Exists
                    || !doc.TryGetValue(path: "sender_id", value: out string senderId)
                    || !doc.TryGetValue(path: "text", value: out string message)
                )
                    continue;

                if (
                    !currentChatroomParticipants.TryGetValue(
                        key: senderId,
                        value: out string? senderName
                    )
                )
                    senderName = "Unknown User";

                string displayName =
                    senderId == SessionHandler.UserId ? $"{senderName} (me)" : senderName;

                bool isPinned = await FirebaseHelper.IsChatPinnedById(
                    SessionHandler.CurrentChatroomId!,
                    message_id: change.Document.Id
                );

                bool isHello = message == "hello";

                string chatEntry =
                    $"{displayName}: {(isHello ? "hello" : ProfanityChecker.CensorTextRobust(text: message))}";

                int existingIndex = message_ids.IndexOf(item: docId);

                if (existingIndex >= 0)
                {
                    // Modified document → update
                    messages[existingIndex] = chatEntry;
                }
                else
                {
                    // New message
                    message_ids.Add(item: docId);
                    messages.Add(item: chatEntry);
                }
            }

            // Rebuild UI with fillers
            bool needsFill = messages.Count < chatHistory.Frame.Height;
            numFill = Math.Max(val1: 0, val2: chatHistory.Frame.Height - messages.Count);
            IEnumerable<string> filler = Enumerable.Repeat(element: ".", count: numFill);
            chatHistory.SetSource(source: needsFill ? [.. filler, .. messages] : messages);

            Application.MainLoop.Invoke(action: ScrollToLatestChat);
        }

        private async Task StartChatroomListener(string chatroom_id)
        {
            DocumentReference chatroomRef = db.Collection(path: "Chatrooms")
                .Document(path: chatroom_id);

            chatroomListener = chatroomRef.Listen(
                callback: async (_) =>
                {
                    string newChatroomName = await FirebaseHelper.GetChatroomNameById(
                        chatroom_id: chatroom_id
                    );

                    currentPinnedIds = await FirebaseHelper.GetChatroomPinnedMessagesIdById(
                        chatroom_id
                    );

                    Application.MainLoop.Invoke(async () =>
                    {
                        chatroomName.Text = newChatroomName;
                        chatroomName.X = Pos.Center();

                        List<string> pinnedMessagesId =
                            await FirebaseHelper.GetChatroomPinnedMessagesIdById(chatroom_id);

                        for (int i = 0; i < messages.Count; i++)
                        {
                            bool isPinned = currentPinnedIds.Contains(message_ids[i]);
                            string messageEntry = messages[i];

                            if (string.IsNullOrEmpty(messageEntry))
                                continue;

                            char lastCharacter = messageEntry[^1];

                            if (isPinned)
                            {
                                if (lastCharacter == '•')
                                    continue;

                                messages[i] += "•";
                            }
                            else if (lastCharacter == '•')
                                messages[i] = messageEntry[..^1];
                        }

                        bool needsFill = messages.Count < chatHistory.Frame.Height;
                        numFill = Math.Max(0, chatHistory.Frame.Height - messages.Count);
                        IEnumerable<string> filler = Enumerable.Repeat(".", numFill);
                        chatHistory.SetSource(
                            source: needsFill ? [.. filler, .. messages] : messages
                        );
                    });
                }
            );
        }

        private void ScrollToLatestChat()
        {
            chatHistory.ScrollDown(items: messages.Count - 1);
            chatHistory.ScrollUp(items: chatHistory.Frame.Height - 1);
        }
    }
}
