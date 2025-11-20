using Google.Cloud.Firestore;
using Terminal.Gui;

namespace Banter
{
    public sealed class ViewPinnedMessagesWindow : AbstractWindow
    {
        private static readonly Lazy<ViewPinnedMessagesWindow> lazyInstance = new(
            valueFactory: () =>
                new ViewPinnedMessagesWindow()
        );
        private readonly FirestoreDb db = FirestoreManager.Instance.Database;
        private FirestoreChangeListener? chatroomListener;

        public static ViewPinnedMessagesWindow Instance => lazyInstance.Value;

        private readonly List<string> message_ids = [];
        private readonly List<string> messages = [];
        private int numFill = 0;

        private ViewPinnedMessagesWindow()
        {
            window.Add(views: [pinnedMessagesListView, closeButton]);

            closeButton.Clicked += Hide;

            window.Leave += (_) =>
            {
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

        public void Show()
        {
            WindowHelper.FocusWindow(window: window);
            StartPinnedMessagesListener(SessionHandler.CurrentChatroomId!);
        }

        public void Hide()
        {
            chatroomListener?.StopAsync();
            chatroomListener = null;

            pinnedMessagesListView.SetSource(new List<string>());

            messages.Clear();
            message_ids.Clear();
            Application.Top.Remove(view: window);
        }

        private readonly Window window = new()
        {
            Title = "Pinned messages",

            Height = Dim.Percent(n: 50),
            Width = Dim.Percent(n: 50),

            X = Pos.Center(),
            Y = Pos.Center(),

            ColorScheme = CustomColorScheme.Window,
        };

        private readonly ListView pinnedMessagesListView = new()
        {
            X = Pos.At(0),
            Y = Pos.At(2),

            Width = Dim.Fill(),
            Height = Dim.Fill(),
        };

        Button closeButton = new()
        {
            Text = "Close",

            X = Pos.AnchorEnd() - Pos.At("Close".Length + 4),
            Y = 0,

            HotKeySpecifier = (Rune)0xffff,
        };

        private void StartPinnedMessagesListener(string chatroom_id)
        {
            DocumentReference chatroomRef = db.Collection("Chatrooms").Document(chatroom_id);

            chatroomListener = chatroomRef.Listen(callback: OnSnapshotReceived);
        }

        private void OnSnapshotReceived(DocumentSnapshot snapshot)
        {
            _ = HandleSnapshotAsync(snapshot); // fire-and-forget
        }

        private async Task HandleSnapshotAsync(DocumentSnapshot snapshot)
        {
            if (
                !snapshot.Exists
                || !snapshot.TryGetValue("pinned_messages", out List<string> firePinnedIds)
            )
            {
                firePinnedIds = [];
            }

            // 1. Remove anything that was unpinned
            for (int i = message_ids.Count - 1; i >= 0; i--)
            {
                if (!firePinnedIds.Contains(message_ids[i]))
                {
                    message_ids.RemoveAt(i);
                    messages.RemoveAt(i);
                }
            }

            // 2. Add anything that was newly pinned
            foreach (string message_id in firePinnedIds)
            {
                if (!message_ids.Contains(message_id))
                {
                    string? text = await FirebaseHelper.GetChatroomMessageById(
                        SessionHandler.CurrentChatroomId!,
                        message_id
                    );

                    if (!string.IsNullOrEmpty(text))
                    {
                        //TODO: What if message_id isn't found?

                        message_ids.Add(message_id);
                        messages.Add(text);
                    }
                }
            }

            // 3. Re-fill / redraw
            bool needsFill = messages.Count < pinnedMessagesListView.Frame.Height;
            numFill = Math.Max(0, pinnedMessagesListView.Frame.Height - messages.Count);
            IEnumerable<string> fill = Enumerable.Repeat(".", numFill);

            Application.MainLoop.Invoke(() =>
            {
                pinnedMessagesListView.SetSource(needsFill ? [.. fill, .. messages] : messages);

                ScrollToLatestChat();
            });
        }

        private void ScrollToLatestChat()
        {
            pinnedMessagesListView.ScrollDown(items: messages.Count - 1);
            pinnedMessagesListView.ScrollUp(items: pinnedMessagesListView.Frame.Height - 1);
        }
    }
}
