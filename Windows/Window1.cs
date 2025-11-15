using Terminal.Gui;

namespace CpE261FinalProject
{
    public sealed class Window1 : AbstractWindow
    {
        // Singleton pattern
        private static readonly Lazy<Window1> lazyInstance = new(() => new Window1());

        public static Window1 Instance => lazyInstance.Value;

        private Window1()
        {
            // Listeners to the session
            SessionHandler.IsLoggedInChanged += Toggle;
            SessionHandler.UserChatroomsChanged += (_) => OnUserChatroomsChanged();
            createChatroomButton.Clicked += async () => await OnCreateChatroomButtonClicked();
            logOutButton.Clicked += async () => await OnLogOutButtonClicked();

            // Initialize
            Toggle(IsLoggedIn: SessionHandler.IsLoggedIn);
        }

        private static async Task OnCreateChatroomButtonClicked()
        {
            CreateChatroomWindow.Instance.Show();
        }

        private static async Task OnLogOutButtonClicked()
        {
            int selectedButton = MessageBox.Query(
                title: "Log out",
                message: "Are you sure you want to log out?",
                buttons: ["Yes", "No"]
            );

            if (selectedButton == 0)
            {
                await SessionHandler.ClearSession();

                Window1.Instance.Hide();
                Window2.Instance.Hide();
                Window3.Instance.Hide();

                LogInWindow.Instance.Show();
            }
        }

        private async void OnUserChatroomsChanged() =>
            Application.MainLoop.Invoke(action: () =>
            {
                labelNumberChatrooms.Text = $"Chatrooms: {SessionHandler.Chatrooms.Count}";
                chatroomsListView.SetSource(
                    source: SessionHandler
                        .Chatrooms.Select(selector: chatroom => chatroom.chatroom_name)
                        .ToList()
                );
            });

        private void Toggle(bool IsLoggedIn)
        {
            //? Isn't this a bit too long?
            Application.MainLoop.Invoke(action: () =>
            {
                window.RemoveAll();

                if (!IsLoggedIn)
                {
                    window.Add(view: labelNotLoggedIn);
                    return;
                }

                if (!IsLoggedIn)
                    return;

                // Initialize user's information
                // Dynamically set the size
                List<string> information =
                [
                    $"Username: {SessionHandler.Username}",
                    $"Name: {SessionHandler.Name}",
                    $"SenderId: {SessionHandler.UserId}",
                ];
                informationListView.SetSource(source: information);
                informationListView.Height = Dim.Sized(n: information.Count);
                informationListView.Y = Pos.AnchorEnd(margin: information.Count + 1);

                // Dynamically set the Y-position
                // Initialize chatroom count
                labelNumberChatrooms.Y = Pos.Top(view: informationListView) - 1;
                labelNumberChatrooms.Text = $"Chatrooms: {SessionHandler.Chatrooms.Count}";

                // Add an event listener to the chatrooms buttons
                // Initialize the height
                chatroomsListView.SetSource(
                    SessionHandler
                        .Chatrooms.Select(selector: chatroom => chatroom.chatroom_id)
                        .ToList()
                );
                chatroomsListView.SelectedItemChanged += (_) =>
                {
                    int selectedIndex = chatroomsListView.SelectedItem;
                    string selectedChatroom = SessionHandler.Chatrooms[selectedIndex].chatroom_id;
                    SessionHandler.CurrentChatroomId = selectedChatroom;
                };
                chatroomsListView.Height =
                    Dim.Fill()
                    - Dim.Height(view: informationListView)
                    - Dim.Sized(n: 1)
                    - Dim.Height(view: logOutButton);

                window.Add(
                    views:
                    [
                        createChatroomButton,
                        chatroomsListView,
                        labelNumberChatrooms,
                        informationListView,
                        logOutButton,
                    ]
                );
            });
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
            Title = "People",

            Height = Dim.Fill(),
            Width = Dim.Percent(n: 23),

            X = 0,
            Y = 0,

            ColorScheme = CustomColorScheme.Window,
        };

        private readonly Label labelNotLoggedIn = new()
        {
            Text = "Nothing to see here...",

            X = Pos.Center(),
            Y = Pos.Center(),

            ColorScheme = CustomColorScheme.LabelEmpty,
        };

        private static readonly Button createChatroomButton = new()
        {
            Text = "+ Add Chatroom",
            X = Pos.At(n: 0),
            Y = Pos.At(n: 0),

            Width = Dim.Fill(),

            ColorScheme = CustomColorScheme.Button,
        };

        private static readonly ListView chatroomsListView = new()
        {
            Width = Dim.Fill(),
            // Height = Dim.Fill() - Dim.Height(informationListView) - Dim.Sized(1), dynamically set

            X = Pos.At(n: 0),
            Y = Pos.Bottom(view: createChatroomButton) + Pos.At(n: 1),
        };

        private readonly Label labelNumberChatrooms = new() { X = 0 };
        private static readonly ListView informationListView = new()
        {
            X = Pos.At(n: 0),
            // Y = Pos.Percent(75), this can vary so it is not set here

            Width = Dim.Fill(),
            // Height = Dim.Sized(3), hight is dynamic to it is not set here
        };

        private readonly Button logOutButton = new()
        {
            Text = "Log out",

            X = Pos.Center(),
            Y = Pos.Bottom(view: informationListView),

            ColorScheme = CustomColorScheme.Button,
        };
    }
}
