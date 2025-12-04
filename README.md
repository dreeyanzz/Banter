# Banter

> **Where modernity embraces tradition.** \> A real-time, cloud-native chat application living strictly inside your terminal.

[](https://github.com/dreeyanzz/Banter)
[](https://opensource.org/licenses/MIT)
[](https://github.com/gui-cs/Terminal.Gui)

## 📖 Overview

**Banter** is a Text User Interface (TUI) chat application that bridges the gap between the nostalgic aesthetic of BBS/IRC systems and modern cloud architecture.

Unlike standard console applications that rely on simple scrolling text, Banter leverages **Terminal.Gui** to provide a rich, windowed interface—complete with mouse support, dialogs, and menus—all within your command line. Under the hood, it powers real-time communication using **Google Cloud Firestore** and ensures snappy performance with **LiteDB** local caching.

## ✨ Key Features

  * **🖥️ Rich TUI Experience**: Full windowing system, mouse support, and keyboard navigation inside the terminal.
  * **⚡ Real-Time Sync**: Instant messaging powered by **Google Firestore**.
  * **💾 Local Caching**: Uses **LiteDB** to store sessions and data locally, ensuring instant startup and offline capability.
  * **🛡️ Smart Moderation**: Built-in `ProfanityChecker` to keep conversations clean.
  * **🔐 Secure Auth**: Robust login and registration system.
  * **💬 Modern Chat Features**: Message history, and responsive UI.

## 🛠️ Tech Stack

| Component | Technology | Description |
| :--- | :--- | :--- |
| **Language** | C\# / .NET | Core application logic. |
| **UI Framework** | [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui) | The TUI windowing toolkit. |
| **Backend** | Google Firestore | NoSQL cloud database for real-time syncing. |
| **Persistence** | LiteDB | Embedded NoSQL database for local caching. |

## 📂 Project Structure

A clean separation of concerns ensures the codebase is easy to navigate and maintain.

```text
Banter/
├── 📄 Banter.csproj          # Project configuration
├── 📄 Program.cs             # Entry point & app initialization
├── 📂 Utilities/             # Logic & Backend Layer
│   ├── 📄 FirebaseHelper.cs  # Firestore connection & sync logic
│   ├── 📄 ProfanityChecker.cs# Content filtering algorithms
│   └── 📄 SessionHandler.cs  # LiteDB local caching & auth state
└── 📂 Windows/               # UI / Presentation Layer
    ├── 📄 LoginWindow.cs     # Authentication UI
    ├── 📄 RegisterWindow.cs  # User registration UI
    └── 📄 ChatWindow.cs      # Main chat interface & message rendering
```

## 🚀 Getting Started

### Prerequisites

  * [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later.
  * A Google Cloud Project with **Firestore** enabled.

### Installation

1.  **Clone the repository**

    ```bash
    git clone https://github.com/dreeyanzz/Banter.git
    cd Banter
    ```

2.  **Configuration**

      * The project requires Firebase credentials.
      * Open `Utilities/FirebaseHelper.cs` and update it with your project's API details (or place your `firebase_config.json` if configured to read from file).

3.  **Build and Run**

    ```bash
    dotnet restore
    dotnet build
    dotnet run
    ```

## 🕹️ Controls

  * **Mouse**: You can click buttons, select text, and switch windows using your mouse.
  * **Enter**: Send message or activate button.

## 🤝 Contributing

Contributions are welcome\! If you have ideas for new features (like file sharing, emojis, or direct messages), feel free to fork the repo and submit a Pull Request.

1.  Fork the Project
2.  Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3.  Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4.  Push to the Branch (`git push origin feature/AmazingFeature`)
5.  Open a Pull Request

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

-----

> Made with ❤️ by **dreeyanzz**
