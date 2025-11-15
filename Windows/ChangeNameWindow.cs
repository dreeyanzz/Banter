using Terminal.Gui;

namespace CpE261FinalProject
{
    public sealed class ChangeChatroomNameWindow : AbstractWindow
    {
        private static readonly Lazy<ChangeChatroomNameWindow> lazyInstance = new(
            valueFactory: () =>
                new ChangeChatroomNameWindow()
        );

        public static ChangeChatroomNameWindow Instance => lazyInstance.Value;

        private ChangeChatroomNameWindow()
        {
            closeButton.Clicked += Hide;
            setButton.Clicked += async () => await OnSetButtonClicked();

            window.Leave += (_) => Hide();
            window.Enter += (_) => Application.MainLoop.Invoke(action: () => dummyView.SetFocus());

            window.Add(views: [newNameTextField, setButton, closeButton]);
        }

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

        public void Show()
        {
            WindowHelper.FocusWindow(window: window);
        }

        public void Hide()
        {
            newNameTextField.Text = string.Empty;
            Application.Top.Remove(view: window);
        }

        private readonly Window window = new()
        {
            Title = "Change chatroom name",

            Height = Dim.Percent(n: 50),
            Width = Dim.Percent(n: 50),

            X = Pos.Center(),
            Y = Pos.Center(),

            ColorScheme = CustomColorScheme.Window,
        };

        private readonly Button closeButton = new()
        {
            Text = "Close",

            X = Pos.AnchorEnd() - Pos.At("Close".Length + 4),
            Y = Pos.At(0),

            HotKeySpecifier = (Rune)0xffff,

            ColorScheme = CustomColorScheme.Button,
        };

        private readonly TextField newNameTextField = new()
        {
            X = 0,
            Y = Pos.Center(),

            Width = Dim.Fill() - Dim.Width(view: setButton),
        };

        private static readonly Button setButton = new()
        {
            Text = "Set",

            X = Pos.AnchorEnd() - Pos.At(n: "Set".Length + 4),
            Y = Pos.Center(),

            HotKeySpecifier = (Rune)0xffff,
        };
    }
}
