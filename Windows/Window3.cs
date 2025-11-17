using Terminal.Gui;

namespace CpE261FinalProject
{
    public sealed class Window3 : AbstractWindow
    {
        private static readonly Lazy<Window3> lazyInstance = new(() => new Window3());

        public static Window3 Instance => lazyInstance.Value;

        private readonly List<View> views = [clearMessagesButton];

        private Window3()
        {
            clearMessagesButton.Clicked += async () => await OnClearMessagesButtonClicked();
            deleteChatroomButton.Clicked += async () => await OnDeleteChatroomButtonClicked();
            SessionHandler.CurrentChatroomChanged += async (_) => await OnCurrentChatroomChanged();
            leaveChatroomButton.Clicked += async () => await OnLeaveChatroomButton();
            changeChatroomNameButton.Clicked += async () =>
                await OnChangeChatroomNameButtonClicked();

            window.Enter += (_) => Application.MainLoop.Invoke(action: () => dummyView.SetFocus());

            _ = OnCurrentChatroomChanged(); // Initialize
        }

        private static async Task OnLeaveChatroomButton()
        {
            int buttonClicked = MessageBox.Query(
                title: "Message",
                message: "Are you sure you want to leave this chatroom?",
                buttons: ["Yes", "No"]
            );

            if (buttonClicked == 0)
                await FirebaseHelper.RemoveChatroomParticipant( //! two `!` here!
                    participant_id: SessionHandler.UserId!,
                    chatroom_id: SessionHandler.CurrentChatroomId!
                );
        }

        private async Task OnCurrentChatroomChanged()
        {
            window.RemoveAll();
            ChangeChatroomNameWindow.Instance.Hide();

            if (string.IsNullOrEmpty(value: SessionHandler.CurrentChatroomId))
                return;

            string chatroom_type = await FirebaseHelper.GetChatroomTypeById(
                chatroom_id: SessionHandler.CurrentChatroomId
            );

            if (chatroom_type == "group")
            {
                bool isChatroomAdmin = await FirebaseHelper.ValidateChatroomAdmin(
                    user_id: SessionHandler.UserId! //! using `!` here
                );
                window.Add(view: changeChatroomNameButton);
                window.Add(view: leaveChatroomButton);
                window.Add();
                if (isChatroomAdmin)
                    window.Add(view: deleteChatroomButton);
            }

            window.Add(views: [.. views]);
        }

        private static async Task OnChangeChatroomNameButtonClicked()
        {
            ChangeChatroomNameWindow.Instance.Show();
        }

        private static async Task OnClearMessagesButtonClicked()
        {
            int buttonClicked = MessageBox.Query(
                title: "Message",
                message: "Are you sure you want to clear messages?\nThis deletes for all of the participants in this chatroom.",
                buttons: ["Yes", "No"]
            );

            if (buttonClicked == 0)
                await FirebaseHelper.ClearChatroomMessagesById(
                    chatroom_id: SessionHandler.CurrentChatroomId! //! using `!` here!
                );
        }

        private static async Task OnDeleteChatroomButtonClicked()
        {
            int buttonClicked = MessageBox.Query(
                title: "Message",
                message: "Are you sure you want to delete this chatroom?",
                buttons: ["Yes", "No"]
            );

            if (buttonClicked == 0)
            {
                await FirebaseHelper.DeleteChatroomById(
                    chatroom_id: SessionHandler.CurrentChatroomId! //! using `!` here!
                );
                SessionHandler.CurrentChatroomId = null;
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
            Title = "Chat info",

            Height = Dim.Fill(),
            Width = Dim.Percent(n: 23),

            X = Pos.Percent(n: 77),
            Y = 0,

            ColorScheme = CustomColorScheme.Window,
        };

        private static readonly Button clearMessagesButton = new()
        {
            Text = "Clear Messages",

            X = Pos.Center(),
            Y = Pos.Center() - Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };

        private static readonly Button changeChatroomNameButton = new()
        {
            Text = "Change chatroom name",

            X = Pos.Center(),
            Y = Pos.Y(view: clearMessagesButton) + Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };

        private static readonly Button deleteChatroomButton = new()
        {
            Text = "Delete Chatroom",

            X = Pos.Center(),
            Y = Pos.AnchorEnd() - Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };

        private static readonly Button leaveChatroomButton = new()
        {
            Text = "Leave Chatroom",

            X = Pos.Center(),
            Y = Pos.Y(view: deleteChatroomButton) - Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };
    }
}
