using Terminal.Gui;

namespace CpE261FinalProject
{
    public sealed class CreateChatroomWindow : AbstractWindow
    {
        // Singleton pattern
        private static readonly Lazy<CreateChatroomWindow> lazyInstance = new(() =>
            new CreateChatroomWindow()
        );

        public static CreateChatroomWindow Instance => lazyInstance.Value;

        private readonly List<string> participants = [];
        private List<string> participants_ids = [];

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

            participants_ids.Add(item: participant_id!);
            participants.Add(item: inputUsername);
            usersTextField.Text = string.Empty;
        }

        private async Task OnCreateButtonClicked()
        {
            if (participants.Count <= 0)
                return;

            await FirebaseHelper.CreateChatroom(participants_ids: participants_ids);

            Hide();
            participants.Clear();
            participants_ids.Clear();
        }

        public void Show()
        {
            WindowHelper.FocusWindow(window: window);
        }

        public void Hide()
        {
            Application.Top.Remove(view: window);
        }

        private readonly Window window = new()
        {
            Title = "Create Chatroom",

            Height = Dim.Percent(50),
            Width = Dim.Percent(50),

            X = Pos.Center(),
            Y = Pos.Center(),

            ColorScheme = CustomColorScheme.Window,
        };

        private readonly Label indicatorLabel = new() { Text = "Users to be added:" };

        private readonly Button closeButton = new()
        {
            Text = "Close",

            X = Pos.AnchorEnd() - Pos.At("Close".Length + 4),
            Y = Pos.At(0),

            ColorScheme = CustomColorScheme.Button,
        };

        private readonly ListView participantsListView = new()
        {
            Height = 5,
            Width = Dim.Fill(),

            X = 0,
            Y = Pos.At(2),
        };

        private readonly Label inputUsernameLabel = new()
        {
            Text = "Type usernames here:",

            X = 0,
            Y = Pos.AnchorEnd() - Pos.At(4),
        };

        private readonly TextField usersTextField = new()
        {
            Y = Pos.AnchorEnd() - Pos.At(3),

            Width = Dim.Fill() - Dim.Width(addButton),
        };

        private static readonly Button addButton = new()
        {
            Text = "Add",

            X = Pos.AnchorEnd() - Pos.At("Add".Length + 4),
            Y = Pos.AnchorEnd() - Pos.At(3),

            ColorScheme = CustomColorScheme.Button,
        };
        private static readonly Button removeLastButton = new()
        {
            Text = "Remove Last",

            X = 0,
            Y = Pos.AnchorEnd() - Pos.At(n: 1),

            ColorScheme = CustomColorScheme.Button,
        };
        private readonly Button clearButton = new()
        {
            Text = "Clear",

            X = Pos.Right(view: removeLastButton),
            Y = Pos.AnchorEnd() - Pos.At(n: 1),

            ColorScheme = CustomColorScheme.Button,
        };
        private readonly Button createButton = new()
        {
            Text = "Create",

            X = Pos.AnchorEnd() - Pos.At("Create".Length + 4),
            Y = Pos.AnchorEnd() - Pos.At(1),

            ColorScheme = CustomColorScheme.Button,
        };
    }
}
