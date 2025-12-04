Banter

<div align="center">

<!-- You can replace this image link with a screenshot of your TUI later -->

<img src="https://www.google.com/search?q=https://raw.githubusercontent.com/dreeyanzz/Banter/main/Banter_Class_Diagram.png" alt="Banter Class Diagram" width="600"/>

Where modernity embraces tradition.

Download Installer • Banter Hub Website

</div>

📖 Overview

Banter is a real-time, terminal-based chat application (TUI) built with C# and .NET. It bridges the gap between the nostalgic aesthetic of BBS/IRC systems and modern cloud architecture.

Unlike standard console applications that rely on scrolling text, Banter leverages Terminal.Gui to provide a rich, windowed interface—complete with mouse support, dialogs, and menus—strictly inside your terminal.

✨ Features

💻 TUI Interface: A keyboard-centric, retro experience built on Terminal.Gui.

☁️ Cloud Native: Powered by Google Cloud Firestore for real-time message syncing across clients.

⚡ Local Caching: Utilizes LiteDB to cache user sessions and data locally, ensuring instant load times.

🔒 Secure Authentication: Robust login and account creation system.

🛡️ Smart Moderation: Integrated ProfanityChecker to automatically filter inappropriate content.

📌 Chat Management: Admin tools for pinning messages and managing rooms.

🛠️ Tech Stack

Component

Technology

Description

Core

C# / .NET

The backbone of the application.

UI Framework

Terminal.Gui

Provides the TUI (Text User Interface) windowing system.

Database

Google Firestore

Scalable NoSQL cloud database for real-time data.

Caching

LiteDB

Embedded NoSQL database for local persistence.

Architecture

MVvm-ish

Clean separation of UI (Windows/) and Logic (Utilities/).

🚀 Installation

Option 1: The Installer (Recommended)

Download the standalone installer which handles all dependencies and creates a desktop shortcut.
<br />
👉 Download BanterSetup.exe

Option 2: Build from Source

If you are a developer and want to contribute:

Clone the repository

git clone [https://github.com/dreeyanzz/Banter.git](https://github.com/dreeyanzz/Banter.git)
cd Banter


Configuration

The project uses an embedded firebase_config.json.

To use your own database, update Utilities/FirebaseHelper.cs with your credentials.

Build & Run

dotnet restore
dotnet build
dotnet run


📂 Project Structure

A look at the source architecture:

Banter/
├── Banter.csproj          # Project configuration & Dependencies
├── Program.cs             # Application Entry Point
├── BanterLogo.txt         # ASCII Art Assets
├── Utilities/             # Backend Logic Layer
│   ├── FirebaseHelper.cs    # Firestore connection & sync
│   ├── ProfanityChecker.cs  # Content filtering algorithms
│   └── SessionHandler.cs    # LiteDB local caching logic
└── Windows/               # UI Presentation Layer
    ├── LoginWindow.cs       # Authentication Views
    ├── RegisterWindow.cs    # Account Creation Views
    └── ChatWindow.cs        # Main Chat Interface


🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are greatly appreciated.

Fork the Project

Create your Feature Branch (git checkout -b feature/AmazingFeature)

Commit your Changes (git commit -m 'Add some AmazingFeature')

Push to the Branch (git push origin feature/AmazingFeature)

Open a Pull Request

📄 License

Distributed under the MIT License. See LICENSE for more information.

<div align="center">

Made with ❤️ by Adrian Seth Tabotabo

</div>
