using Banter.Utilities;
using Terminal.Gui;

namespace Banter.Windows
{
    /// <summary>
    /// Represents the login window of the application. This class is a singleton.
    /// </summary>
    public sealed class LogInWindow : AbstractWindow
    {
        // Singleton pattern
        private static readonly Lazy<LogInWindow> lazyInstance = new(valueFactory: () =>
            new LogInWindow()
        );

        /// <summary>
        /// Gets the singleton instance of the <see cref="LogInWindow"/>.
        /// </summary>
        public static LogInWindow Instance => lazyInstance.Value;

        private LogInWindow()
        {
            DisplayLogo();

            showHidePassword.Text = passwordTextField.Secret == true ? "Hide" : "Show"; // Initialize
            showHidePassword.Clicked += () =>
            {
                showHidePassword.Text = passwordTextField.Secret == true ? "Hide" : "Show";
                passwordTextField.Secret = !passwordTextField.Secret;
            };

            logInButton.Clicked += OnLogInButtonClicked;
            createAccountButton.Clicked += OnCreateAccountButtonClicked;

            window.Enter += (_) => logInButton.IsDefault = true;
            window.Leave += (_) => logInButton.IsDefault = false;

            window.Add(
                views:
                [
                    usernameLabel,
                    usernameTextField,
                    passwordLabel,
                    passwordTextField,
                    showHidePassword,
                    logInButton,
                    createAccountButton,
                ]
            );
        }

        /// <summary>
        /// Shows the window.
        /// </summary>
        public void Show()
        {
            WindowHelper.OpenWindow(window: window);
            WindowHelper.FocusWindow(window: window);
        }

        /// <summary>
        /// Hides the window.
        /// </summary>
        public void Hide()
        {
            WindowHelper.CloseWindow(window: window);
        }

        /// <summary>
        /// Displays the Banter logo.
        /// </summary>
        private void DisplayLogo()
        {
            List<string> BanterLogo = [.. File.ReadAllLines(path: "BanterLogo.txt")];
            BanterLogo.Insert(index: 0, item: new string(c: ' ', count: BanterLogo[0].Length));
            this.BanterLogo.SetSource(source: BanterLogo);
            this.BanterLogo.Height = BanterLogo.Count;
            this.BanterLogo.Width = BanterLogo[1].Length;
            window.Add(view: this.BanterLogo);
        }

        /// <summary>
        /// Enables or disables all interactive UI elements.
        /// </summary>
        /// <param name="isEnabled">Whether the elements should be enabled.</param>
        private void SetInteractables(bool isEnabled)
        {
            usernameTextField.Enabled = isEnabled;
            passwordTextField.Enabled = isEnabled;
            logInButton.Enabled = isEnabled;
        }

        /// <summary>
        /// Handles the click event of the "Log In" button.
        /// </summary>
        private async void OnLogInButtonClicked()
        {
            string inputUsername = usernameTextField.Text.ToString() ?? string.Empty;
            string inputPassword = passwordTextField.Text.ToString() ?? string.Empty;

            if (
                string.IsNullOrEmpty(value: inputUsername)
                || string.IsNullOrEmpty(value: inputPassword)
            )
            {
                MessageBox.Query(
                    title: "Message",
                    message: "Username or Password cannot be empty!",
                    buttons: ["Ok"]
                );
                return;
            }

            logInButton.Text = "Logging in...";
            SetInteractables(isEnabled: false);

            string user_id =
                await FirebaseHelper.GetUserIdFromUsername(username: inputUsername) ?? string.Empty;
            if (string.IsNullOrEmpty(value: user_id))
            {
                MessageBox.Query(title: "Message", message: "Account not found.", buttons: ["Ok"]);

                logInButton.Text = "Log In";
                SetInteractables(isEnabled: true);

                return;
            }

            Dictionary<string, object> user_info = await FirebaseHelper.GetUserInfoById(
                user_id: user_id
            );

            if (user_info.Count == 0)
            {
                MessageBox.Query(
                    title: "Message",
                    message: "Something went wrong...",
                    buttons: ["Ok"]
                );

                logInButton.Text = "Log In";
                SetInteractables(isEnabled: true);

                return;
            }

            if (!user_info.TryGetValue(key: "password", value: out object? dbPassword)) //Using nullable here
            {
                MessageBox.Query(
                    title: "Message",
                    message: "Something went wrong...",
                    buttons: ["Ok"]
                );

                logInButton.Text = "Log In";
                SetInteractables(isEnabled: true);

                return;
            }

            string password = (string)dbPassword;
            bool isPasswordMatch = password == inputPassword;
            if (!isPasswordMatch)
            {
                MessageBox.Query(title: "Message", message: "Wrong password...", buttons: ["Ok"]);

                logInButton.Text = "Log In";
                SetInteractables(isEnabled: true);

                return;
            }

            SessionHandler.UserId = user_id;
            if (user_info.TryGetValue(key: "name", value: out object? name))
                SessionHandler.Name = (string)name;

            if (user_info.TryGetValue(key: "username", value: out object? username))
                SessionHandler.Username = (string)username;

            SessionHandler.IsLoggedIn = true;

            await SessionHandler.StartChatroomsListener();

            logInButton.Text = "Log In";
            SetInteractables(isEnabled: true);
            usernameTextField.Text = string.Empty;
            passwordTextField.Text = string.Empty;
            WindowHelper.CloseWindow(window: window);

            // Show the three main windows
            Window1.Instance.Show();
            Window2.Instance.Show();
            Window3.Instance.Show();
        }

        /// <summary>
        /// Handles the click event of the "Create Account" button.
        /// Closes this window and then opens CreateAccountWindow
        /// </summary>
        private void OnCreateAccountButtonClicked()
        {
            WindowHelper.CloseWindow(window: window);
            CreateAccountWindow.Instance.Show();
        }

        /// <summary>
        /// The main window for this view.
        /// </summary>
        private readonly Window window = new()
        {
            Title = "Log In",

            Height = Dim.Fill(),
            Width = Dim.Fill(),

            ColorScheme = CustomColorScheme.Window,
        };

        /// <summary>
        /// The list view for displaying the Banter logo.
        /// </summary>
        private readonly ListView BanterLogo = new()
        {
            X = Pos.Center(),
            Y = Pos.At(n: 2),

            Enabled = false,
        };

        /// <summary>
        /// The label for the username text field.
        /// </summary>
        private readonly Label usernameLabel = new()
        {
            Text = "Username:",

            X = Pos.Left(view: usernameTextField) - Pos.At(n: "Username:".Length + 1),
            Y = Pos.Y(view: usernameTextField),
        };

        /// <summary>
        /// The text field for entering the username.
        /// </summary>
        private static readonly TextField usernameTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Center() - Pos.At(n: 3),

            Width = 50,
        };

        /// <summary>
        /// The label for the password text field.
        /// </summary>
        private readonly Label passwordLabel = new()
        {
            Text = "Password:",

            X = Pos.Left(view: passwordTextField) - Pos.At(n: "Password:".Length + 1),
            Y = Pos.Y(view: passwordTextField),
        };

        /// <summary>
        /// The text field for entering the password.
        /// </summary>
        private static readonly TextField passwordTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Center() - Pos.At(n: 1),

            Width = 50,

            Secret = true,
        };

        /// <summary>
        /// The button to show or hide the password.
        /// </summary>
        private readonly Button showHidePassword = new()
        {
            Text = "",

            X = Pos.Right(view: passwordTextField) + Pos.At(n: 1),
            Y = Pos.Y(view: passwordTextField),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The button to log in.
        /// </summary>
        private readonly Button logInButton = new()
        {
            Text = "Log In",

            X = Pos.Center(),
            Y = Pos.Center() + Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The button to navigate to the create account window.
        /// </summary>
        private readonly Button createAccountButton = new()
        {
            Text = "Create Account",

            X = Pos.Center(),
            Y = Pos.Center() + Pos.At(n: 3),

            HotKeySpecifier = (Rune)0xffff,
        };
    }
}
