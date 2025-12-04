```
▀█████████▄     ▄████████ ███▄▄▄▄       ███        ▄████████    ▄████████
  ███    ███   ███    ███ ███▀▀▀██▄ ▀█████████▄   ███    ███   ███    ███
  ███    ███   ███    ███ ███   ███    ▀███▀▀██   ███    █▀    ███    ███
 ▄███▄▄▄██▀    ███    ███ ███   ███     ███   ▀  ▄███▄▄▄      ▄███▄▄▄▄██▀
▀▀███▀▀▀██▄  ▀███████████ ███   ███     ███     ▀▀███▀▀▀     ▀▀███▀▀▀▀▀
  ███    ██▄   ███    ███ ███   ███     ███       ███    █▄  ▀███████████
  ███    ███   ███    ███ ███   ███     ███       ███    ███   ███    ███
▄█████████▀    ███    █▀   ▀█   █▀     ▄████▀     ██████████   ███    ███
                        where modernity embraces tradition     ███    ███
```

# Banter

Banter is a modern, terminal-based chat application built with C# and .NET. It provides a lightweight and keyboard-centric interface for real-time communication, bringing a classic chat experience to the command line.

## Features

- **User Authentication**: Securely log in or create a new account.
- **Chatrooms**: Create public or private chatrooms to organize conversations.
- **Real-time Messaging**: Send and receive messages instantly.
- **Message Pinning**: Pin important messages to the top of a chatroom for easy access.
- **Chat Management**:
  - Change chatroom names.
  - Leave chatrooms you are a part of.
  - Admins can delete chatrooms.
  - Clear the message history of a chatroom.
- **Profanity Filter**: Automatically censors inappropriate language in messages.
- **Local Caching**: Caches user data locally for improved performance using LiteDB.

## Technologies Used

- **Backend & Core**: C# with .NET
- **Terminal UI**: [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)
- **Database**: Google Cloud Firestore
- **Local Cache**: LiteDB

## Getting Started

Follow these instructions to get a local copy of Banter up and running.

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (Version 10.0 or compatible)
- A Google Cloud Platform project with Firestore enabled.

### Installation & Setup

1.  **Clone the repository:**

    ```sh
    git clone <repository-url>
    cd Banter
    ```

2.  **Set up Firebase:**

    - This project uses Firebase for its backend. You will need to set up your own Firebase project.
    - The application is configured to use an embedded resource for the Firebase credentials (`banter-7717f-firebase-adminsdk-fbsvc-9cb1cd3095.json`).
    - To use your own Firebase project, you need to:
      1.  Go to your Firebase project settings and generate a new private key for the service account. This will download a JSON file.
      2.  Rename the downloaded JSON file to match the one expected by the application or update the path in `Utilities/FirestoreManager.cs`.
      3.  Make sure the JSON file is set as an "Embedded resource" in your project's `.csproj` file.

3.  **Restore Dependencies:**
    Open a terminal in the project root and run:
    ```sh
    dotnet restore
    ```

### Running the Application

1.  **Build the project:**

    ```sh
    dotnet build
    ```

2.  **Run the application:**
    ```sh
    dotnet run
    ```

## Project Structure

The project is organized into the following main directories:

- `Windows/`: Contains the various windows or "screens" of the application, built using `Terminal.Gui`.
- `Utilities/`: Contains helper classes and modules for functionalities like:
  - `FirebaseHelper.cs`: Interacting with the Firestore database.
  - `ProfanityChecker.cs`: Filtering messages.
  - `SessionHandler.cs`: Managing user sessions.
  - And more for caching, validation, etc.
- `Program.cs`: The main entry point of the application.

## License

This project is licensed under the MIT License. See the `LICENSE` file for more details.
