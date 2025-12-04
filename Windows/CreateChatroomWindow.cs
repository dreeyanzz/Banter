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
        private static readonly Lazy<CreateChatroomWindow> lazyInstance = new(valueFactory: () =>
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

            addButton.Clicked += async () => await OnAddButtonClicked();

            removeLastButton.Clicked += () =>
            {
                if (participants.Count <= 0)
                    return;

                participants.RemoveAt(index: participants.Count - 1);
                participants_ids.RemoveAt(index: participants_ids.Count - 1);
            };

            clearButton.Clicked += () =>
            {
                participants.Clear();
                participants_ids.Clear();
            };

            participantsListView.SetSource(source: participants);
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
        /// Handles the click event of the "Add" button.
        /// </summary>
        private async Task OnAddButtonClicked()
        {
            string inputUsername = usersTextField.Text.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(value: inputUsername))
                return;

            bool isOwnUsername = inputUsername == SessionHandler.Username;
            if (isOwnUsername)
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

            string participant_id = await FirebaseHelper.GetUserIdFromUsername(
                username: inputUsername
            );

            participants.Add(item: inputUsername);
            participants_ids.Add(item: participant_id);
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
        /// The main window for this view.
        /// </summary>
        private readonly Window window = new()
        {
            Title = "Create Chatroom",

            Height = Dim.Percent(n: 50),
            Width = Dim.Percent(n: 50),

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

            X = Pos.AnchorEnd() - Pos.At(n: "Close".Length + 4),
            Y = Pos.At(n: 0),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The list view displaying the participants to be added.
        /// </summary>
        private readonly ListView participantsListView = new()
        {
            Height = Dim.Sized(n: 5),
            Width = Dim.Fill(),

            X = Pos.At(n: 0),
            Y = Pos.At(n: 2),
        };

        /// <summary>
        /// The label for the username input field.
        /// </summary>
        private readonly Label inputUsernameLabel = new()
        {
            Text = "Type usernames here:",

            X = Pos.At(n: 0),
            Y = Pos.AnchorEnd() - Pos.At(n: 4),
        };

        /// <summary>
        /// The text field for entering usernames.
        /// </summary>
        private readonly TextField usersTextField = new()
        {
            Y = Pos.AnchorEnd() - Pos.At(n: 3),

            Width = Dim.Fill() - Dim.Width(view: addButton),
        };

        /// <summary>
        /// The button to add a user to the participants list.
        /// </summary>
        private static readonly Button addButton = new()
        {
            Text = "Add",

            X = Pos.AnchorEnd() - Pos.At(n: "Add".Length + 4),
            Y = Pos.AnchorEnd() - Pos.At(n: 3),

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

            X = Pos.AnchorEnd() - Pos.At(n: "Create".Length + 4),
            Y = Pos.AnchorEnd() - Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };
    }
}
