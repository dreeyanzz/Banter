using Banter.Utilities;
using Terminal.Gui;

namespace Banter.Windows
{
    /// <summary>
    /// Represents the window for creating a new chatroom. This class is a singleton.
    /// </summary>
    public sealed class CreateChatroomWindow : AbstractWindow
    {
        // Singleton pattern
        private static readonly Lazy<CreateChatroomWindow> lazyInstance = new(() =>
            new CreateChatroomWindow()
        );

        /// <summary>
        /// Gets the singleton instance of the <see cref="CreateChatroomWindow"/>.
        /// </summary>
        public static CreateChatroomWindow Instance => lazyInstance.Value;

        private readonly List<string> participants = [];
        private readonly List<string> participants_ids = [];

        private CreateChatroomWindow()
        {
            closeButton.Clicked += () =>
            {
                Hide();
                participants.Clear();
                participants_ids.Clear();
            };
            removeLastButton.Clicked += () =>
            {
                if (participants.Count > 0)
                {
                    participants.RemoveAt(index: participants.Count - 1);
                    participants_ids.RemoveAt(index: participants_ids.Count - 1);
                }
            };
            clearButton.Clicked += () =>
            {
                participants.Clear();
                participants_ids.Clear();
            };
            participantsListView.SetSource(source: participants);
            addButton.Clicked += async () => await OnAddButtonClicked();
            createButton.Clicked += async () => await OnCreateButtonClicked();

            window.Enter += (_) =>
                Application.MainLoop.Invoke(action: () => usersTextField.SetFocus());

            window.Leave += (_) =>
            {
                Hide();
                participants.Clear();
                participants_ids.Clear();
            };

            window.Add(
                views:
                [
                    indicatorLabel,
                    closeButton,
                    participantsListView,
                    inputUsernameLabel,
                    usersTextField,
                    addButton,
                    removeLastButton,
                    createButton,
                    clearButton,
                ]
            );
        }

        /// <summary>
        /// Handles the click event of the "Add" button.
        /// </summary>
        private async Task OnAddButtonClicked()
        {
            string? inputUsername = usersTextField.Text.ToString();

            if (string.IsNullOrEmpty(inputUsername))
                return;

            if (inputUsername == SessionHandler.Username)
            {
                MessageBox.ErrorQuery(
                    title: "Error",
                    message: "You cannot add yourself.",
                    buttons: ["Ok"]
                );
                return;
            }

            // Check if username is existent
            bool isExistent = !await FirebaseHelper.IsUsernameTaken(username: inputUsername);
            if (!isExistent)
            {
                MessageBox.ErrorQuery(
                    title: "Error",
                    message: "Username does not exist",
                    buttons: ["Ok"]
                );
                return;
            }

            string? participant_id = await FirebaseHelper.GetUserIdFromUsername(
                username: inputUsername
            );

            participants_ids.Add(item: participant_id!); //! using `!` here
            participants.Add(item: inputUsername);
            usersTextField.Text = string.Empty;
        }

        /// <summary>
        /// Handles the click event of the "Create" button.
        /// </summary>
        private async Task OnCreateButtonClicked()
        {
            if (participants.Count <= 0)
                return;

            await FirebaseHelper.CreateChatroom(participants_ids: participants_ids);

            Hide();
            participants.Clear();
            participants_ids.Clear();
        }

        /// <summary>
        /// Shows the window.
        /// </summary>
        public void Show()
        {
            WindowHelper.FocusWindow(window: window);
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
            Title = "Create Chatroom",

            Height = Dim.Percent(50),
            Width = Dim.Percent(50),

            X = Pos.Center(),
            Y = Pos.Center(),

            ColorScheme = CustomColorScheme.Window,
        };

        /// <summary>
        /// The label indicating the list of users to be added.
        /// </summary>
        private readonly Label indicatorLabel = new() { Text = "Users to be added:" };

        /// <summary>
        /// The button to close the window.
        /// </summary>
        private readonly Button closeButton = new()
        {
            Text = "Close",

            X = Pos.AnchorEnd() - Pos.At("Close".Length + 4),
            Y = Pos.At(0),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The list view displaying the participants to be added.
        /// </summary>
        private readonly ListView participantsListView = new()
        {
            Height = 5,
            Width = Dim.Fill(),

            X = 0,
            Y = Pos.At(2),
        };

        /// <summary>
        /// The label for the username input field.
        /// </summary>
        private readonly Label inputUsernameLabel = new()
        {
            Text = "Type usernames here:",

            X = 0,
            Y = Pos.AnchorEnd() - Pos.At(4),
        };

        /// <summary>
        /// The text field for entering usernames.
        /// </summary>
        private readonly TextField usersTextField = new()
        {
            Y = Pos.AnchorEnd() - Pos.At(3),

            Width = Dim.Fill() - Dim.Width(addButton),
        };

        /// <summary>
        /// The button to add a user to the participants list.
        /// </summary>
        private static readonly Button addButton = new()
        {
            Text = "Add",

            X = Pos.AnchorEnd() - Pos.At("Add".Length + 4),
            Y = Pos.AnchorEnd() - Pos.At(3),

            HotKeySpecifier = (Rune)0xffff,
            IsDefault = true,
        };

        /// <summary>
        /// The button to remove the last added participant.
        /// </summary>
        private static readonly Button removeLastButton = new()
        {
            Text = "Remove Last",

            X = 0,
            Y = Pos.AnchorEnd() - Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The button to clear the participants list.
        /// </summary>
        private readonly Button clearButton = new()
        {
            Text = "Clear",

            X = Pos.Right(view: removeLastButton),
            Y = Pos.AnchorEnd() - Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The button to create the chatroom.
        /// </summary>
        private readonly Button createButton = new()
        {
            Text = "Create",

            X = Pos.AnchorEnd() - Pos.At("Create".Length + 4),
            Y = Pos.AnchorEnd() - Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };
    }
}
