using Banter.Utilities;
using Terminal.Gui;

namespace Banter.Windows
{
    /// <summary>
    /// Represents the leftmost window in the main application UI, displaying chatrooms and user information. This class is a singleton.
    /// </summary>
    public sealed class Window1 : AbstractWindow
    {
        // Singleton pattern
        private static readonly Lazy<Window1> lazyInstance = new(valueFactory: () => new Window1());

        /// <summary>
        /// Gets the singleton instance of the <see cref="Window1"/>.
        /// </summary>
        public static Window1 Instance => lazyInstance.Value;

        private List<string> chatroomNames = [];
        private List<string> chatroomIds = [];

        List<int> filteredIndices = [];
        bool isFiltered = false;
        List<string> filteredNames = [];
        List<string> filteredIds = [];
        private int numFill = 0;

        private Window1()
        {
            SessionHandler.UserChatroomsChanged += (_) => OnUserChatroomsChanged();
            createChatroomButton.Clicked += async () => await OnCreateChatroomButtonClicked();
            logOutButton.Clicked += async () => await OnLogOutButtonClicked();

            OnUserChatroomsChanged();

            List<string> information =
            [
                $"Username: {SessionHandler.Username}",
                $"Name: {SessionHandler.Name}",
                $"SenderId: {SessionHandler.UserId}",
            ];
            informationListView.SetSource(source: information);
            informationListView.Height = Dim.Sized(n: information.Count);
            informationListView.Y = Pos.AnchorEnd() - Pos.At(n: information.Count + 1);

            labelNumberChatrooms.Y = Pos.Y(view: informationListView) - Pos.At(n: 1);
            labelNumberChatrooms.Text = $"Chatrooms: {SessionHandler.Chatrooms.Count}";

            chatroomsListView.SelectedItemChanged += (_) =>
            {
                int selectedIndex = chatroomsListView.SelectedItem;
                if (selectedIndex >= (isFiltered ? filteredNames.Count : chatroomNames.Count))
                    return;

                string selectedChatroom;
                if (isFiltered)
                    selectedChatroom = filteredIds[selectedIndex];
                else
                    selectedChatroom = chatroomIds[selectedIndex];

                SessionHandler.CurrentChatroomId = selectedChatroom;
            };

            Application.MainLoop.Invoke(() =>
            {
                chatroomsListView.Height =
                    Dim.Fill()
                    - Dim.Height(view: informationListView)
                    - Dim.Height(view: logOutButton)
                    - Dim.Sized(n: 2);

                // !CAN BE OPTIMIZED
                bool needsFill = chatroomNames.Count < chatroomsListView.Frame.Height;
                numFill = Math.Max(0, chatroomsListView.Frame.Height - chatroomNames.Count);
                IEnumerable<string> filler = Enumerable.Repeat(element: ".", count: numFill);
                Application.MainLoop.Invoke(action: () =>
                    chatroomsListView.SetSource(
                        source: needsFill ? [.. chatroomNames, .. filler] : chatroomNames
                    )
                );
            });

            searchChatroomTextField.TextChanged += async (_) =>
            {
                string textToSearch = searchChatroomTextField.Text.ToString() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(value: textToSearch))
                {
                    searchIndicator.Text = $"Searching for: {textToSearch}";

                    filteredIndices =
                    [
                        .. chatroomNames
                            .Select(selector: (value, index) => new { value, index })
                            .Where(predicate: x =>
                                x.value.Contains(
                                    value: textToSearch,
                                    comparisonType: StringComparison.OrdinalIgnoreCase
                                )
                            )
                            .Select(selector: x => x.index),
                    ];

                    filteredNames =
                    [
                        .. filteredIndices.Select(selector: index => chatroomNames[index]),
                    ];
                    filteredIds =
                    [
                        .. filteredIndices.Select(selector: index => chatroomIds[index]),
                    ];
                    isFiltered = true;

                    //!OPTIMIZE THIS
                    bool needsFill = filteredNames.Count < chatroomsListView.Frame.Height;
                    numFill = Math.Max(
                        val1: 0,
                        val2: chatroomsListView.Frame.Height - filteredNames.Count
                    );
                    IEnumerable<string> filler = Enumerable.Repeat(element: ".", count: numFill);
                    Application.MainLoop.Invoke(action: () =>
                        chatroomsListView.SetSource(
                            source: needsFill ? [.. filteredNames, .. filler] : filteredNames
                        )
                    );
                }
                else
                {
                    searchIndicator.Text = string.Empty;
                    filteredNames = [];
                    filteredIds = [];
                    isFiltered = false;

                    bool needsFill = chatroomNames.Count < chatroomsListView.Frame.Height;
                    numFill = Math.Max(
                        val1: 0,
                        val2: chatroomsListView.Frame.Height - chatroomNames.Count
                    );
                    IEnumerable<string> filler = Enumerable.Repeat(element: ".", count: numFill);
                    Application.MainLoop.Invoke(action: () =>
                        chatroomsListView.SetSource(
                            source: needsFill ? [.. chatroomNames, .. filler] : chatroomNames
                        )
                    );
                }
            };

            window.Enter += (_) => Application.MainLoop.Invoke(action: () => dummyView.SetFocus());

            window.Add(
                views:
                [
                    createChatroomButton,
                    searchChatroomLabel,
                    searchChatroomTextField,
                    searchIndicator,
                    chatroomsListView,
                    labelNumberChatrooms,
                    informationListView,
                    logOutButton,
                ]
            );
        }

        /// <summary>
        /// Handles the click event of the "Create Chatroom" button.
        /// </summary>
        private static async Task OnCreateChatroomButtonClicked()
        {
            CreateChatroomWindow.Instance.Show();
        }

        /// <summary>
        /// Handles the click event of the "Log Out" button.
        /// </summary>
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

        /// <summary>
        /// Handles the event when the user's chatrooms change.
        /// </summary>
        private async void OnUserChatroomsChanged() =>
            Application.MainLoop.Invoke(action: () =>
            {
                chatroomNames =
                [
                    .. SessionHandler.Chatrooms.Select(chatroom => chatroom.chatroom_name),
                ];
                chatroomIds =
                [
                    .. SessionHandler.Chatrooms.Select(chatroom => chatroom.chatroom_id),
                ];

                // !OPTIMIZE THIS
                bool needsFill = chatroomNames.Count < chatroomsListView.Frame.Height;
                numFill = Math.Max(
                    val1: 0,
                    val2: chatroomsListView.Frame.Height - chatroomNames.Count
                );
                IEnumerable<string> filler = Enumerable.Repeat(element: ".", count: numFill);
                Application.MainLoop.Invoke(action: () =>
                    chatroomsListView.SetSource(
                        source: needsFill ? [.. chatroomNames, .. filler] : chatroomNames
                    )
                );

                labelNumberChatrooms.Text = $"Chatrooms: {chatroomIds.Count}";
            });

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
            Title = "People",

            Height = Dim.Fill(),
            Width = Dim.Percent(n: 23),

            X = 0,
            Y = 0,

            ColorScheme = CustomColorScheme.Window,
        };

        /// <summary>
        /// The button to create a new chatroom.
        /// </summary>
        private static readonly Button createChatroomButton = new()
        {
            Text = "+ Add Chatroom",
            X = Pos.At(n: 0),
            Y = Pos.At(n: 0),

            Width = Dim.Fill(),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The label for the search text field.
        /// </summary>
        private static readonly Label searchChatroomLabel = new()
        {
            Text = "Search here:",

            X = Pos.At(n: 0),
            Y = Pos.At(n: 2),
        };

        /// <summary>
        /// The text field for searching chatrooms.
        /// </summary>
        private static readonly TextField searchChatroomTextField = new()
        {
            X = Pos.At(n: 0),
            Y = Pos.Y(view: searchChatroomLabel) + Pos.At(n: 1),

            Width = Dim.Fill(),
        };

        /// <summary>
        /// The label to indicate the current search query.
        /// </summary>
        private static readonly Label searchIndicator = new()
        {
            Text = string.Empty,

            X = Pos.At(n: 0),
            Y = Pos.Y(view: searchChatroomTextField) + Pos.At(n: 2),
        };

        /// <summary>
        /// The list view for displaying chatrooms.
        /// </summary>
        private static readonly ListView chatroomsListView = new()
        {
            Width = Dim.Fill(),
            // Height = Dim.Fill() - Dim.Height(informationListView) - Dim.Sized(1), dynamically set

            X = Pos.At(n: 0),
            Y = Pos.Y(view: searchIndicator) + Pos.At(n: 1),
        };

        /// <summary>
        /// The label displaying the number of chatrooms.
        /// </summary>
        private readonly Label labelNumberChatrooms = new() { X = 0 };

        /// <summary>
        /// The list view for displaying user information.
        /// </summary>
        private static readonly ListView informationListView = new()
        {
            X = Pos.At(n: 0),
            // Y = Pos.Percent(75), this can vary so it is not set here

            Width = Dim.Fill(),
            // Height = Dim.Sized(3), hight is dynamic to it is not set here
        };

        /// <summary>
        /// The button to log out.
        /// </summary>
        private readonly Button logOutButton = new()
        {
            Text = "Log out",

            X = Pos.Center(),
            Y = Pos.AnchorEnd() - Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };
    }
}
