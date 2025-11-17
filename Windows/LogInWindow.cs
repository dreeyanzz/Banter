using Terminal.Gui;

namespace CpE261FinalProject
{
    public sealed class LogInWindow : AbstractWindow
    {
        // Singleton pattern
        private static readonly Lazy<LogInWindow> lazyInstance = new(() => new LogInWindow());

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

        private void SetInteractables(bool isEnabled)
        {
            usernameTextField.Enabled = isEnabled;
            passwordTextField.Enabled = isEnabled;
            logInButton.Enabled = isEnabled;
        }

        public void Show()
        {
            WindowHelper.OpenWindow(window);
            WindowHelper.FocusWindow(window);
        }

        public void Hide()
        {
            WindowHelper.CloseWindow(window);
        }

        private void DisplayLogo()
        {
            List<string> BanterLogo = [.. File.ReadAllLines("BanterLogo.txt")];
            BanterLogo.Insert(0, new string(' ', BanterLogo[0].Length));
            this.BanterLogo.SetSource(BanterLogo);
            this.BanterLogo.Height = BanterLogo.Count;
            this.BanterLogo.Width = BanterLogo[1].Length;
            window.Add(view: this.BanterLogo);
        }

        private async void OnLogInButtonClicked()
        {
            string? inputUsername = usernameTextField.Text.ToString();
            string? inputPassword = passwordTextField.Text.ToString();

            if (string.IsNullOrEmpty(inputUsername) || string.IsNullOrEmpty(inputPassword))
            {
                MessageBox.Query(
                    title: "Message",
                    message: "Username or Password cannot be empty!",
                    buttons: ["Ok"]
                );
                return;
            }

            logInButton.Text = "Logging in...";
            SetInteractables(false);

            string? user_id = await FirebaseHelper.GetUserIdFromUsername(inputUsername);
            if (user_id == null)
            {
                MessageBox.Query(title: "Message", message: "Account not found.", buttons: ["Ok"]);

                logInButton.Text = "Log In";
                SetInteractables(true);

                return;
            }

            Dictionary<string, object>? user_info = await FirebaseHelper.GetUserInfoById(user_id);

            if (user_info == null)
            {
                MessageBox.Query(
                    title: "Message",
                    message: "Something went wrong...",
                    buttons: ["Ok"]
                );

                logInButton.Text = "Log In";
                SetInteractables(true);

                return;
            }

            string password;
            if (!user_info.TryGetValue("password", out object? dbPassword))
            {
                MessageBox.Query(
                    title: "Message",
                    message: "Something went wrong...",
                    buttons: ["Ok"]
                );

                logInButton.Text = "Log In";
                SetInteractables(true);

                return;
            }
            password = (string)dbPassword;

            bool isPasswordMatch = password == inputPassword;
            if (!isPasswordMatch)
            {
                MessageBox.Query(title: "Message", message: "Wrong password...", buttons: ["Ok"]);

                logInButton.Text = "Log In";
                SetInteractables(true);

                return;
            }

            SessionHandler.UserId = user_id;
            if (user_info.TryGetValue("name", out object? name))
                SessionHandler.Name = (string)name;

            if (user_info.TryGetValue("username", out object? username))
                SessionHandler.Username = (string)username;

            SessionHandler.IsLoggedIn = true;

            await SessionHandler.StartChatroomsListener();

            logInButton.Text = "Log In";
            SetInteractables(true);
            usernameTextField.Text = "";
            passwordTextField.Text = "";
            WindowHelper.CloseWindow(window: window);

            Window1.Instance.Show();
            Window2.Instance.Show();
            Window3.Instance.Show();
        }

        private void OnCreateAccountButtonClicked()
        {
            WindowHelper.CloseWindow(window: window);

            CreateAccountWindow.Instance.Show();
        }

        private readonly Window window = new()
        {
            Title = "Log In",

            Height = Dim.Fill(),
            Width = Dim.Fill(),

            ColorScheme = CustomColorScheme.Window,
        };

        private readonly ListView BanterLogo = new()
        {
            X = Pos.Center(),
            Y = Pos.At(2),

            Enabled = false,
        };

        private readonly Label usernameLabel = new()
        {
            Text = "Username:",

            X = Pos.Left(usernameTextField) - Pos.At("Username:".Length + 1),
            Y = Pos.Y(usernameTextField),
        };

        private static readonly TextField usernameTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Center() - Pos.At(3),

            Width = 50,
        };

        private readonly Label passwordLabel = new()
        {
            Text = "Password:",

            X = Pos.Left(passwordTextField) - Pos.At("Password:".Length + 1),
            Y = Pos.Y(passwordTextField),
        };

        private static readonly TextField passwordTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Center() - Pos.At(1),

            Width = 50,

            Secret = true,
        };
        private readonly Button showHidePassword = new()
        {
            Text = "",

            X = Pos.Right(passwordTextField) + Pos.At(1),
            Y = Pos.Y(passwordTextField),

            HotKeySpecifier = (Rune)0xffff,
        };

        private readonly Button logInButton = new()
        {
            Text = "Log In",

            X = Pos.Center(),
            Y = Pos.Center() + Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };

        private readonly Button createAccountButton = new()
        {
            Text = "Create Account",

            X = Pos.Center(),
            Y = Pos.Center() + Pos.At(n: 3),

            HotKeySpecifier = (Rune)0xffff,
        };
    }
}
