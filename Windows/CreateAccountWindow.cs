using Banter.Utilities;
using Terminal.Gui;

namespace Banter.Windows
{
    /// <summary>
    /// Represents the window for creating a new user account. This class is a singleton.
    /// </summary>
    public sealed class CreateAccountWindow : AbstractWindow
    {
        // Singleton pattern
        private static readonly Lazy<CreateAccountWindow> lazyInstance = new(valueFactory: () =>
            new CreateAccountWindow()
        );

        /// <summary>
        /// Gets the singleton instance of the <see cref="CreateAccountWindow"/>.
        /// </summary>
        public static CreateAccountWindow Instance => lazyInstance.Value;

        private CreateAccountWindow()
        {
            DisplayLogo();

            SetInteractables(isEnabled: true);

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

            window.Enter += (_) => createAccountButton.IsDefault = true;
            window.Leave += (_) => createAccountButton.IsDefault = false;

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
            ClearFields();
        }

        /// <summary>
        /// Clears all input fields.
        /// </summary>
        private static void ClearFields()
        {
            usernameTextField.Text = string.Empty;
            passwordTextField.Text = string.Empty;
            repeatPasswordTextField.Text = string.Empty;
            nameTextField.Text = string.Empty;
            emailTextField.Text = string.Empty;
        }

        /// <summary>
        /// Enables or disables all interactive UI elements.
        /// </summary>
        /// <param name="isEnabled">Whether the elements should be enabled.</param>
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

        /// <summary>
        /// Displays a validation error message.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private void HandleValidationError(string message)
        {
            MessageBox.ErrorQuery(title: "Invalid input", message: message, buttons: ["Ok"]);

            createAccountButton.Text = "Create Account";
            SetInteractables(isEnabled: true);
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
        /// The list view for displaying the Banter logo.
        /// </summary>
        private readonly ListView BanterLogo = new()
        {
            X = Pos.Center(),
            Y = Pos.At(n: 2),

            Enabled = false,
        };

        /// <summary>
        /// Handles the click event of the "Back" button.
        /// </summary>
        private void OnBackButtonClicked()
        {
            ClearFields();

            Hide();
            LogInWindow.Instance.Show();
        }

        /// <summary>
        /// Handles the click event of the "Create Account" button.
        /// </summary>
        private async void OnCreateAccountButtonClicked()
        {
            string inputUsername = usernameTextField.Text.ToString() ?? string.Empty;
            string inputPassword = passwordTextField.Text.ToString() ?? string.Empty;
            string inputRepeatPassword = repeatPasswordTextField.Text.ToString() ?? string.Empty;
            string inputName = nameTextField.Text.ToString() ?? string.Empty;
            string inputEmail = emailTextField.Text.ToString() ?? string.Empty;

            if (
                string.IsNullOrEmpty(value: inputUsername)
                || string.IsNullOrEmpty(value: inputPassword)
                || string.IsNullOrEmpty(value: inputRepeatPassword)
            )
            {
                HandleValidationError(message: "Username and Passwords cannot be empty!");
                return;
            }

            createAccountButton.Text = "Creating Account...";
            SetInteractables(isEnabled: false);

            if (inputUsername.Length < 8)
            {
                HandleValidationError(message: "Username must be atleast 8 characters long");
                return;
            }

            if (!await FirebaseHelper.IsUsernameTaken(username: inputUsername))
            {
                HandleValidationError(message: "Username already taken");
                return;
            }

            if (inputPassword.Length < 8)
            {
                HandleValidationError(message: "Password must be atleast 8 characters long");
                return;
            }

            if (string.IsNullOrEmpty(value: inputName))
            {
                HandleValidationError(message: "Name cannot be empty!");
                return;
            }

            if (string.IsNullOrEmpty(value: inputEmail))
            {
                HandleValidationError(message: "Email cannot be empty!");
                return;
            }

            if (!Validator.IsValidEmail(email: inputEmail))
            {
                HandleValidationError(message: "Invalid email format!");
                return;
            }

            if (inputPassword != inputRepeatPassword)
            {
                HandleValidationError(message: "Passwords must match!");
                return;
            }

            User user = new(
                email: inputEmail,
                name: inputName,
                password: inputPassword,
                username: inputUsername
            );
            if (!await FirebaseHelper.AddAccount(user: user))
            {
                MessageBox.ErrorQuery(
                    title: "Service Error",
                    message: "Something went wrong creating your account. Try again later.",
                    buttons: ["Ok"]
                );

                createAccountButton.Text = "Create Account";
                SetInteractables(isEnabled: true);

                return;
            }
            MessageBox.Query(
                title: "Service Error",
                message: "Account creating success!",
                buttons: ["Ok"]
            );

            createAccountButton.Text = "Create Account";
            SetInteractables(isEnabled: true);
            ClearFields();
        }

        /// <summary>
        /// The main window for this view.
        /// </summary>
        private readonly Window window = new()
        {
            Title = "Create Account",

            Height = Dim.Fill(),
            Width = Dim.Fill(),

            ColorScheme = CustomColorScheme.Window,
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
        /// The label for the repeat password text field.
        /// </summary>
        private readonly Label repeatPasswordLabel = new()
        {
            Text = "Repeat password:",

            X = Pos.Left(view: repeatPasswordTextField) - Pos.At(n: "Repeat password:".Length + 1),
            Y = Pos.Y(view: repeatPasswordTextField),
        };

        /// <summary>
        /// The text field for repeating the password.
        /// </summary>
        private static readonly TextField repeatPasswordTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Center() + Pos.At(n: 1),

            Width = 50,

            Secret = true,
        };

        /// <summary>
        /// The button to show or hide the passwords.
        /// </summary>
        private static readonly Button showHidePasswords = new()
        {
            Text = string.Empty,

            X = Pos.Center(),
            Y = Pos.Bottom(view: repeatPasswordTextField) + Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The label for the name text field.
        /// </summary>
        private readonly Label nameLabel = new()
        {
            Text = "Name:",

            X = Pos.Left(view: nameTextField) - Pos.At(n: "Name:".Length + 1),
            Y = Pos.Y(view: nameTextField),
        };

        /// <summary>
        /// The text field for entering the name.
        /// </summary>
        private static readonly TextField nameTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(view: showHidePasswords) + Pos.At(n: 1),

            Width = 50,

            Secret = false,
        };

        /// <summary>
        /// The label for the email text field.
        /// </summary>
        private readonly Label emailLabel = new()
        {
            Text = "Email:",

            X = Pos.Left(view: emailTextField) - Pos.At(n: "Email:".Length + 1),
            Y = Pos.Y(view: emailTextField),
        };

        /// <summary>
        /// The text field for entering the email.
        /// </summary>
        private static readonly TextField emailTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(view: nameTextField) + Pos.At(n: 1),

            Width = 50,

            Secret = false,
        };

        /// <summary>
        /// The button to create the account.
        /// </summary>
        private readonly Button createAccountButton = new()
        {
            Text = "Create Account",

            X = Pos.Percent(n: 65) - Pos.At(n: 20),
            Y = Pos.Bottom(view: emailTextField) + Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The button to go back to the login window.
        /// </summary>
        private readonly Button backButton = new()
        {
            Text = "Back",

            X = Pos.Percent(n: 35),
            Y = Pos.Bottom(view: emailTextField) + Pos.At(n: 1),

            HotKeySpecifier = (Rune)0xffff,
        };
    }
}
