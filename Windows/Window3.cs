using Banter.Utilities;
using Terminal.Gui;

namespace Banter.Windows
{
    /// <summary>
    /// Represents the rightmost window in the main application UI, displaying chatroom information and management options. This class is a singleton.
    /// </summary>
    public sealed class Window3 : AbstractWindow
    {
        private static readonly Lazy<Window3> lazyInstance = new(valueFactory: () => new Window3());

        /// <summary>
        /// Gets the singleton instance of the <see cref="Window3"/>.
        /// </summary>
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

        /// <summary>
        /// Handles the click event of the "Leave Chatroom" button.
        /// </summary>
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

        /// <summary>
        /// Handles the current chatroom changed event.
        /// </summary>
        private async Task OnCurrentChatroomChanged()
        {
            window.RemoveAll();
            ChangeChatroomNameWindow.Instance.Hide();

            if (string.IsNullOrEmpty(value: SessionHandler.CurrentChatroomId))
                return;

            string chatroom_type = await FirebaseHelper.GetChatroomTypeById(
                chatroom_id: SessionHandler.CurrentChatroomId
            );

            //? What if chatroom_type is empty?

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

        /// <summary>
        /// Handles the click event of the "Change Chatroom Name" button.
        /// </summary>
        private static async Task OnChangeChatroomNameButtonClicked()
        {
            ChangeChatroomNameWindow.Instance.Show();
        }

        /// <summary>
        /// Handles the click event of the "Clear Messages" button.
        /// </summary>
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

        /// <summary>
        /// Handles the click event of the "Delete Chatroom" button.
        /// </summary>
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

        /// <summary>
        /// Shows the window.
        /// </summary>
        public void Show()
        {
            Application.Top.Add(views: [window]);
        }

        /// <summary>
        /// Hides the window.
        /// </summary>
        public void Hide()
        {
            Application.Top.Remove(view: window);
        }

        /// <summary>
        /// The main window for this view.
        /// </summary>
        private readonly Window window = new()
        {
            Title = "Chat info",

            Height = Dim.Fill(),
            Width = Dim.Percent(n: 23),

            X = Pos.Percent(n: 77),
            Y = 0,

            ColorScheme = CustomColorScheme.Window,
        };

        /// <summary>
        /// The button to clear all messages in the chatroom.
        /// </summary>
        private static readonly Button clearMessagesButton = new()
        {
            Text = "Clear Messages",

            X = Pos.Center(),
            Y = Pos.Center() - Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The button to change the chatroom name.
        /// </summary>
        private static readonly Button changeChatroomNameButton = new()
        {
            Text = "Change chatroom name",

            X = Pos.Center(),
            Y = Pos.Y(view: clearMessagesButton) + Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The button to delete the chatroom.
        /// </summary>
        private static readonly Button deleteChatroomButton = new()
        {
            Text = "Delete Chatroom",

            X = Pos.Center(),
            Y = Pos.AnchorEnd() - Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The button to leave the chatroom.
        /// </summary>
        private static readonly Button leaveChatroomButton = new()
        {
            Text = "Leave Chatroom",

            X = Pos.Center(),
            Y = Pos.Y(view: deleteChatroomButton) - Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };
    }
}
