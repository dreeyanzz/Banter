using Banter.Utilities;
using Terminal.Gui;

namespace Banter.Windows
{
    /// <summary>
    /// Represents the window for changing a chatroom's name. This class is a singleton.
    /// </summary>
    public sealed class ChangeChatroomNameWindow : AbstractWindow
    {
        private static readonly Lazy<ChangeChatroomNameWindow> lazyInstance = new(
            valueFactory: () =>
                new ChangeChatroomNameWindow()
        );

        /// <summary>
        /// Gets the singleton instance of the <see cref="ChangeChatroomNameWindow"/>.
        /// </summary>
        public static ChangeChatroomNameWindow Instance => lazyInstance.Value;

        private ChangeChatroomNameWindow()
        {
            closeButton.Clicked += Hide;
            setButton.Clicked += async () => await OnSetButtonClicked();

            window.Enter += (_) => setButton.IsDefault = true;

            window.Leave += (_) =>
            {
                setButton.IsDefault = false;
                Hide();
            };
            window.Enter += (_) => Application.MainLoop.Invoke(action: () => dummyView.SetFocus());

            window.Add(views: [newNameTextField, setButton, closeButton]);
        }

        /// <summary>
        /// Handles the click event of the "Set" button.
        /// </summary>
        private async Task OnSetButtonClicked()
        {
            string? inputName = newNameTextField.Text.ToString();

            if (string.IsNullOrEmpty(inputName))
                return;

            await FirebaseHelper.ChangeChatroomName(
                chatroom_id: SessionHandler.CurrentChatroomId!, //! Using `!` here
                new_name: inputName
            );

            Hide();
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
            newNameTextField.Text = string.Empty;
            Application.Top.Remove(view: window);
        }

        /// <summary>
        /// The main window for this view.
        /// </summary>
        private readonly Window window = new()
        {
            Title = "Change chatroom name",

            Height = Dim.Percent(n: 50),
            Width = Dim.Percent(n: 50),

            X = Pos.Center(),
            Y = Pos.Center(),

            ColorScheme = CustomColorScheme.Window,
        };

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
        /// The text field for entering the new chatroom name.
        /// </summary>
        private readonly TextField newNameTextField = new()
        {
            X = 0,
            Y = Pos.Center(),

            Width = Dim.Fill() - Dim.Width(view: setButton),
        };

        /// <summary>
        /// The button to set the new chatroom name.
        /// </summary>
        private static readonly Button setButton = new()
        {
            Text = "Set",

            X = Pos.AnchorEnd() - Pos.At(n: "Set".Length + 4),
            Y = Pos.Center(),

            HotKeySpecifier = (Rune)0xffff,
        };
    }
}
