using Banter.Utilities;
using Google.Cloud.Firestore;
using Terminal.Gui;

namespace Banter.Windows
{
    /// <summary>
    /// Represents the window for viewing pinned messages in a chatroom. This class is a singleton.
    /// </summary>
    public sealed class ViewPinnedMessagesWindow : AbstractWindow
    {
        private static readonly Lazy<ViewPinnedMessagesWindow> lazyInstance = new(
            valueFactory: () =>
                new ViewPinnedMessagesWindow()
        );

        /// <summary>
        /// Gets the singleton instance of the <see cref="ViewPinnedMessagesWindow"/>.
        /// </summary>
        public static ViewPinnedMessagesWindow Instance => lazyInstance.Value;

        private readonly FirestoreDb db = FirestoreManager.Instance.Database;
        private FirestoreChangeListener chatroomListener;
        private readonly List<string> message_ids = [];
        private readonly List<string> messages = [];
        private int numFill = 0;

        private ViewPinnedMessagesWindow()
        {
            window.Add(views: [pinnedMessagesListView, closeButton]);

            closeButton.Clicked += Hide;

            window.Leave += (_) =>
            {
                // Does not work
                Hide();
            };

            pinnedMessagesListView.OpenSelectedItem += async (_) =>
            {
                int selectedIndex = pinnedMessagesListView.SelectedItem - numFill;

                if (selectedIndex < 0)
                    return;

                string chat_id = message_ids[selectedIndex];
                await FirebaseHelper.RemovePinChatroomMessage(
                    chatroom_id: SessionHandler.CurrentChatroomId!,
                    message_id: chat_id
                ); //! USING `!` here!
            };
        }

        /// <summary>
        /// Shows the window and starts listening for pinned messages.
        /// </summary>
        public void Show()
        {
            WindowHelper.FocusWindow(window: window);
            StartPinnedMessagesListener(chatroom_id: SessionHandler.CurrentChatroomId!); //! Using `!` here
        }

        /// <summary>
        /// Hides the window and stops listening for pinned messages.
        /// </summary>
        public void Hide()
        {
            chatroomListener?.StopAsync();
            chatroomListener = null;

            WindowHelper.CloseWindow(window: window);
            pinnedMessagesListView.SetSource(source: new List<string>()); // Clear source
            messages.Clear();
            message_ids.Clear();
        }

        /// <summary>
        /// The main window for this view.
        /// </summary>
        private readonly Window window = new()
        {
            Title = "Pinned messages",

            Height = Dim.Percent(n: 50),
            Width = Dim.Percent(n: 50),

            X = Pos.Center(),
            Y = Pos.Center(),

            ColorScheme = CustomColorScheme.Window,
        };

        /// <summary>
        /// The list view for displaying pinned messages.
        /// </summary>
        private readonly ListView pinnedMessagesListView = new()
        {
            X = Pos.At(n: 0),
            Y = Pos.At(n: 2),

            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        /// <summary>
        /// The button to close the window.
        /// </summary>
        readonly Button closeButton = new()
        {
            Text = "Close",

            X = Pos.AnchorEnd() - Pos.At(n: "Close".Length + 4),
            Y = 0,

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// Starts listening for changes to pinned messages in a chatroom.
        /// </summary>
        /// <param name="chatroom_id">The ID of the chatroom.</param>
        private void StartPinnedMessagesListener(string chatroom_id)
        {
            DocumentReference chatroomRef = db.Collection(path: "Chatrooms")
                .Document(path: chatroom_id);
            chatroomListener = chatroomRef.Listen(callback: OnSnapshotReceived);
        }

        /// <summary>
        /// Handles the snapshot received from the Firestore listener.
        /// </summary>
        /// <param name="snapshot">The document snapshot.</param>
        private void OnSnapshotReceived(DocumentSnapshot snapshot)
        {
            _ = HandleSnapshotAsync(snapshot: snapshot); // fire-and-forget
        }

        /// <summary>
        /// Asynchronously handles the snapshot to update the pinned messages list.
        /// </summary>
        /// <param name="snapshot">The document snapshot.</param>
        private async Task HandleSnapshotAsync(DocumentSnapshot snapshot)
        {
            if (
                !(
                    snapshot.Exists
                    && snapshot.TryGetValue(
                        path: "pinned_messages",
                        value: out List<string> firePinnedIds
                    )
                )
            )
            {
                firePinnedIds = [];
            }

            // 1. Remove anything that was unpinned
            for (int i = message_ids.Count - 1; i >= 0; i--)
            {
                if (!firePinnedIds.Contains(item: message_ids[i]))
                {
                    message_ids.RemoveAt(index: i);
                    messages.RemoveAt(index: i);
                }
            }

            // 2. Add anything that was newly pinned
            foreach (string message_id in firePinnedIds)
            {
                if (!message_ids.Contains(item: message_id))
                {
                    string text =
                        await FirebaseHelper.GetChatroomMessageById(
                            chatroom_id: SessionHandler.CurrentChatroomId!,
                            message_id: message_id
                        ) ?? string.Empty;

                    if (!string.IsNullOrEmpty(value: text))
                    {
                        //TODO: What if message_id isn't found?
                        message_ids.Add(item: message_id);
                        messages.Add(item: text);
                    }
                }
            }

            // 3. Re-fill / redraw
            bool needsFill = messages.Count < pinnedMessagesListView.Frame.Height;
            numFill = Math.Max(val1: 0, val2: pinnedMessagesListView.Frame.Height - messages.Count);
            IEnumerable<string> fill = Enumerable.Repeat(element: ".", count: numFill);

            Application.MainLoop.Invoke(action: () =>
            {
                pinnedMessagesListView.SetSource(
                    source: needsFill ? [.. fill, .. messages] : messages
                );
                ScrollToLatestChat();
            });
        }

        /// <summary>
        /// Scrolls the pinned messages list to the latest chat.
        /// </summary>
        private void ScrollToLatestChat()
        {
            pinnedMessagesListView.ScrollDown(items: messages.Count - 1);
            pinnedMessagesListView.ScrollUp(items: pinnedMessagesListView.Frame.Height - 1);
        }
    }
}
