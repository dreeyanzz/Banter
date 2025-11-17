using Terminal.Gui;

namespace CpE261FinalProject
{
    public sealed class CreateAccountWindow : AbstractWindow
    {
        // Singleton pattern
        private static readonly Lazy<CreateAccountWindow> lazyInstance = new(() =>
            new CreateAccountWindow()
        );

        public static CreateAccountWindow Instance => lazyInstance.Value;

        private CreateAccountWindow()
        {
            DisplayLogo();

            backButton.Clicked += OnBackButtonClicked;
            createAccountButton.Clicked += OnCreateAccountButtonClicked;

            showHidePasswords.Text =
                passwordTextField.Secret == true ? "Hide Passwords" : "Show Passwords";
            showHidePasswords.Clicked += () =>
            {
                showHidePasswords.Text =
                    passwordTextField.Secret == true ? "Hide Passwords" : "Show Passwords";

                passwordTextField.Secret = !passwordTextField.Secret;
                repeatPasswordTextField.Secret = !repeatPasswordTextField.Secret;
            };

            window.Enter += (_) => Application.MainLoop.Invoke(action: () => dummyView.SetFocus());

            window.Add(
                views:
                [
                    // Username
                    usernameLabel,
                    usernameTextField,
                    // Password
                    passwordLabel,
                    passwordTextField,
                    // Repeat Password
                    repeatPasswordLabel,
                    repeatPasswordTextField,
                    // Show/Hide Passwords button
                    showHidePasswords,
                    // Name
                    nameLabel,
                    nameTextField,
                    // Email
                    emailLabel,
                    emailTextField,
                    // Action Buttons
                    createAccountButton,
                    backButton,
                ]
            );
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

        private void ClearFields()
        {
            usernameTextField.Text = "";
            passwordTextField.Text = "";
            repeatPasswordTextField.Text = "";
            nameTextField.Text = "";
            emailTextField.Text = "";
        }

        private void SetInteractables(bool isEnabled)
        {
            usernameTextField.Enabled = isEnabled;
            passwordTextField.Enabled = isEnabled;
            repeatPasswordTextField.Enabled = isEnabled;
            showHidePasswords.Enabled = isEnabled;
            nameTextField.Enabled = isEnabled;
            emailTextField.Enabled = isEnabled;
            backButton.Enabled = isEnabled;
            createAccountButton.Enabled = isEnabled;
        }

        private void HandleValidationError(string message)
        {
            MessageBox.ErrorQuery(title: "Invalid input", message: message, buttons: ["Ok"]);

            createAccountButton.Text = "Create Account";
            SetInteractables(true);
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

        private readonly ListView BanterLogo = new()
        {
            X = Pos.Center(),
            Y = Pos.At(2),

            Enabled = false,
        };

        private void OnBackButtonClicked()
        {
            ClearFields();

            WindowHelper.CloseWindow(window: window);
            LogInWindow.Instance.Show();
        }

        private async void OnCreateAccountButtonClicked()
        {
            string? inputUsername = usernameTextField.Text.ToString();
            string? inputPassword = passwordTextField.Text.ToString();
            string? inputRepeatPassword = repeatPasswordTextField.Text.ToString();
            string? inputName = nameTextField.Text.ToString();
            string? inputEmail = emailTextField.Text.ToString();

            if (
                string.IsNullOrEmpty(inputUsername)
                || string.IsNullOrEmpty(inputPassword)
                || string.IsNullOrEmpty(inputRepeatPassword)
            )
            {
                HandleValidationError("Username and Passwords cannot be empty!");
                return;
            }

            createAccountButton.Text = "Creating Account...";
            SetInteractables(false);

            if (inputUsername.Length < 8)
            {
                HandleValidationError("Username must be atleast 8 characters long");
                return;
            }

            if (!await FirebaseHelper.IsUsernameTaken(inputUsername))
            {
                HandleValidationError("Username already taken");
                return;
            }

            if (inputPassword.Length < 8)
            {
                HandleValidationError("Password must be atleast 8 characters long");
                return;
            }

            if (string.IsNullOrEmpty(inputName))
            {
                HandleValidationError("Name cannot be empty!");
                return;
            }

            if (string.IsNullOrEmpty(inputEmail))
            {
                HandleValidationError("Email cannot be empty!");
                return;
            }

            if (!Validator.IsValidEmail(inputEmail))
            {
                HandleValidationError("Invalid email format!");
                return;
            }

            if (inputPassword != inputRepeatPassword)
            {
                HandleValidationError("Passwords must match!");
                return;
            }

            User user = new(
                email: inputEmail,
                name: inputName,
                password: inputPassword,
                username: inputUsername
            );
            if (!await FirebaseHelper.AddAccount(user))
            {
                MessageBox.ErrorQuery(
                    title: "Service Error",
                    message: "Something went wrong creating your account. Try again later.",
                    buttons: ["Ok"]
                );

                createAccountButton.Text = "Create Account";
                SetInteractables(true);

                return;
            }
            MessageBox.Query(
                title: "Service Error",
                message: "Account creating success!",
                buttons: ["Ok"]
            );

            createAccountButton.Text = "Create Account";
            SetInteractables(true);
            ClearFields();
        }

        private readonly Window window = new()
        {
            Title = "Create Account",

            Height = Dim.Fill(),
            Width = Dim.Fill(),

            ColorScheme = CustomColorScheme.Window,
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

        private readonly Label repeatPasswordLabel = new()
        {
            Text = "Repeat password:",

            X = Pos.Left(repeatPasswordTextField) - Pos.At("Repeat password:".Length + 1),
            Y = Pos.Y(repeatPasswordTextField),
        };

        private static readonly TextField repeatPasswordTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Center() + Pos.At(1),

            Width = 50,

            Secret = true,
        };

        private static readonly Button showHidePasswords = new()
        {
            Text = "",

            X = Pos.Center(),
            Y = Pos.Bottom(repeatPasswordTextField) + Pos.At(1),

            HotKeySpecifier = (Rune)0xffff,
        };

        private readonly Label nameLabel = new()
        {
            Text = "Name:",

            X = Pos.Left(nameTextField) - Pos.At("Name:".Length + 1),
            Y = Pos.Y(nameTextField),
        };

        private static readonly TextField nameTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(showHidePasswords) + Pos.At(1),

            Width = 50,

            Secret = false,
        };

        private readonly Label emailLabel = new()
        {
            Text = "Email:",

            X = Pos.Left(emailTextField) - Pos.At("Email:".Length + 1),
            Y = Pos.Y(emailTextField),
        };

        private static readonly TextField emailTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(nameTextField) + Pos.At(1),

            Width = 50,

            Secret = false,
        };

        private readonly Button createAccountButton = new()
        {
            Text = "Create Account",

            X = Pos.Percent(65) - Pos.At(20),
            Y = Pos.Bottom(emailTextField) + Pos.At(1),

            HotKeySpecifier = (Rune)0xffff,
        };

        private readonly Button backButton = new()
        {
            Text = "Back",

            X = Pos.Percent(35),
            Y = Pos.Bottom(emailTextField) + Pos.At(1),

            HotKeySpecifier = (Rune)0xffff,
        };
    }
}
