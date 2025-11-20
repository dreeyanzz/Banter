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
        private static readonly Lazy<CreateAccountWindow> lazyInstance = new(() =>
            new CreateAccountWindow()
        );

        /// <summary>
        /// Gets the singleton instance of the <see cref="CreateAccountWindow"/>.
        /// </summary>
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

        /// <summary>
        /// Shows the window.
        /// </summary>
        public void Show()
        {
            WindowHelper.OpenWindow(window);
            WindowHelper.FocusWindow(window);
        }

        /// <summary>
        /// Hides the window.
        /// </summary>
        public void Hide()
        {
            WindowHelper.CloseWindow(window);
        }

        /// <summary>
        /// Clears all input fields.
        /// </summary>
        private void ClearFields()
        {
            usernameTextField.Text = "";
            passwordTextField.Text = "";
            repeatPasswordTextField.Text = "";
            nameTextField.Text = "";
            emailTextField.Text = "";
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
            SetInteractables(true);
        }

        /// <summary>
        /// Displays the Banter logo.
        /// </summary>
        private void DisplayLogo()
        {
            List<string> BanterLogo = [.. File.ReadAllLines("BanterLogo.txt")];
            BanterLogo.Insert(0, new string(' ', BanterLogo[0].Length));
            this.BanterLogo.SetSource(BanterLogo);
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
            Y = Pos.At(2),

            Enabled = false,
        };

        /// <summary>
        /// Handles the click event of the "Back" button.
        /// </summary>
        private void OnBackButtonClicked()
        {
            ClearFields();

            WindowHelper.CloseWindow(window: window);
            LogInWindow.Instance.Show();
        }

        /// <summary>
        /// Handles the click event of the "Create Account" button.
        /// </summary>
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

            X = Pos.Left(usernameTextField) - Pos.At("Username:".Length + 1),
            Y = Pos.Y(usernameTextField),
        };

        /// <summary>
        /// The text field for entering the username.
        /// </summary>
        private static readonly TextField usernameTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Center() - Pos.At(3),

            Width = 50,
        };

        /// <summary>
        /// The label for the password text field.
        /// </summary>
        private readonly Label passwordLabel = new()
        {
            Text = "Password:",

            X = Pos.Left(passwordTextField) - Pos.At("Password:".Length + 1),
            Y = Pos.Y(passwordTextField),
        };

        /// <summary>
        /// The text field for entering the password.
        /// </summary>
        private static readonly TextField passwordTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Center() - Pos.At(1),

            Width = 50,

            Secret = true,
        };

        /// <summary>
        /// The label for the repeat password text field.
        /// </summary>
        private readonly Label repeatPasswordLabel = new()
        {
            Text = "Repeat password:",

            X = Pos.Left(repeatPasswordTextField) - Pos.At("Repeat password:".Length + 1),
            Y = Pos.Y(repeatPasswordTextField),
        };

        /// <summary>
        /// The text field for repeating the password.
        /// </summary>
        private static readonly TextField repeatPasswordTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Center() + Pos.At(1),

            Width = 50,

            Secret = true,
        };

        /// <summary>
        /// The button to show or hide the passwords.
        /// </summary>
        private static readonly Button showHidePasswords = new()
        {
            Text = "",

            X = Pos.Center(),
            Y = Pos.Bottom(repeatPasswordTextField) + Pos.At(1),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The label for the name text field.
        /// </summary>
        private readonly Label nameLabel = new()
        {
            Text = "Name:",

            X = Pos.Left(nameTextField) - Pos.At("Name:".Length + 1),
            Y = Pos.Y(nameTextField),
        };

        /// <summary>
        /// The text field for entering the name.
        /// </summary>
        private static readonly TextField nameTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(showHidePasswords) + Pos.At(1),

            Width = 50,

            Secret = false,
        };

        /// <summary>
        /// The label for the email text field.
        /// </summary>
        private readonly Label emailLabel = new()
        {
            Text = "Email:",

            X = Pos.Left(emailTextField) - Pos.At("Email:".Length + 1),
            Y = Pos.Y(emailTextField),
        };

        /// <summary>
        /// The text field for entering the email.
        /// </summary>
        private static readonly TextField emailTextField = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(nameTextField) + Pos.At(1),

            Width = 50,

            Secret = false,
        };

        /// <summary>
        /// The button to create the account.
        /// </summary>
        private readonly Button createAccountButton = new()
        {
            Text = "Create Account",

            X = Pos.Percent(65) - Pos.At(20),
            Y = Pos.Bottom(emailTextField) + Pos.At(1),

            HotKeySpecifier = (Rune)0xffff,
        };

        /// <summary>
        /// The button to go back to the login window.
        /// </summary>
        private readonly Button backButton = new()
        {
            Text = "Back",

            X = Pos.Percent(35),
            Y = Pos.Bottom(emailTextField) + Pos.At(1),

            HotKeySpecifier = (Rune)0xffff,
        };
    }
}
