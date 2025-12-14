# 🗨️ Banter

> **Where modernity embraces tradition.**  
> A real-time, cloud-native chat application that lives entirely in your terminal.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Terminal.Gui](https://img.shields.io/badge/Terminal.Gui-1.x-brightgreen)](https://github.com/gui-cs/Terminal.Gui)

---

## 📖 Overview

**Banter** is a feature-rich Text User Interface (TUI) chat application that bridges the nostalgic aesthetics of classic BBS/IRC systems with modern cloud architecture. Unlike traditional console applications that rely on simple scrolling text, Banter provides a sophisticated windowed interface—complete with mouse support, interactive dialogs, and dynamic menus—all within your command line.

Powered by **Google Cloud Firestore** for real-time synchronization and **Terminal.Gui** for an immersive terminal experience, Banter delivers instant messaging with enterprise-grade features in a lightweight, keyboard-friendly package.

---

## ✨ Key Features

### 💬 **Real-Time Communication**

- **Instant Messaging**: Messages sync across all clients in real-time via Firestore
- **Group & Individual Chats**: Create private conversations or group chatrooms
- **Message Pinning**: Pin important messages for easy reference
- **Search & Filter**: Search through chat history and chatrooms instantly

### 🖥️ **Rich Terminal Interface**

- **Full Windowing System**: Multiple resizable windows with mouse support
- **Keyboard Navigation**: Efficient hotkey system for power users
- **Responsive Design**: Adaptive layouts that work in various terminal sizes
- **Custom Color Schemes**: Eye-friendly color schemes optimized for extended use

### 🛡️ **Smart Moderation**

- **Profanity Filter**: Built-in content filtering with robust leetspeak detection
- **Admin Controls**: Chatroom admins can manage messages and participants
- **Content Censorship**: Automatically censors inappropriate language

### 👥 **User Management**

- **Secure Authentication**: Login and registration system with validation
- **User Profiles**: Display names and usernames
- **Session Management**: Persistent sessions with automatic cleanup

### 🎛️ **Chatroom Management**

- **Create Chatrooms**: Invite multiple users to group conversations
- **Admin Privileges**: Designated admins can rename, delete, or clear chatrooms
- **Leave/Remove**: Users can leave chatrooms; admins can remove participants
- **Dynamic Updates**: Chatroom lists update in real-time

---

## 🏗️ Architecture

Banter follows a clean, event-driven architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                     Terminal.Gui (View Layer)               │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │ Window1  │  │ Window2  │  │ Window3  │  │  Other   │     │
│  │(Chatroom)│  │  (Chat)  │  │  (Info)  │  │ Windows  │     │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    SessionHandler (State)                   │
│  • Event-driven state management                            │
│  • Real-time Firestore listeners                            │
│  • User session tracking                                    │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                  Utilities (Business Logic)                 │
│  ┌──────────────────┐  ┌──────────────────┐                 │
│  │ FirebaseHelper   │  │ ProfanityChecker │                 │
│  │ • CRUD operations│  │ • Content filter │                 │
│  │ • Real-time sync │  │ • Leetspeak det. │                 │
│  └──────────────────┘  └──────────────────┘                 │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                  Google Cloud Firestore                     │
│  • NoSQL Database • Real-time Updates • Scalable            │
└─────────────────────────────────────────────────────────────┘
```

### Design Patterns

- **Singleton Pattern**: All windows and managers use thread-safe lazy singletons
- **Observer Pattern**: Event-driven updates via `SessionHandler` events
- **Repository Pattern**: `FirebaseHelper` abstracts all database operations

---

## 🛠️ Tech Stack

| Component          | Technology                                             | Purpose                        |
| ------------------ | ------------------------------------------------------ | ------------------------------ |
| **Language**       | C# / .NET 10.0                                         | Core application logic         |
| **UI Framework**   | [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui) | Terminal windowing toolkit     |
| **Backend**        | Google Cloud Firestore                                 | Real-time NoSQL database       |
| **Local Cache**    | LiteDB                                                 | Embedded database for sessions |
| **Authentication** | Firebase Admin SDK                                     | Secure credential management   |

---

## 📂 Project Structure

```
Banter/
├── 📄 Program.cs                    # Application entry point & initialization
│
├── 📂 Utilities/                    # Business Logic Layer
│   ├── FirebaseHelper.cs            # Firestore CRUD operations
│   ├── FirestoreManager.cs          # Singleton Firestore connection
│   ├── SessionHandler.cs            # User session & state management
│   ├── ProfanityChecker.cs          # Content moderation & filtering
│   ├── Validator.cs                 # Input validation (email, etc.)
│   ├── Models.cs                    # Data models (User, Chatroom, Message)
│   ├── Interfaces.cs                # IViewable interface
│   ├── CustomColorScheme.cs         # UI color schemes
│   └── WindowHelper.cs              # Window management utilities
│
├── 📂 Windows/                      # Presentation Layer
│   ├── AbstractWindow.cs            # Base window class
│   ├── LogInWindow.cs               # User authentication UI
│   ├── CreateAccountWindow.cs       # Registration UI
│   ├── Window1.cs                   # Chatroom list & user info
│   ├── Window2.cs                   # Main chat interface
│   ├── Window3.cs                   # Chatroom management panel
│   ├── CreateChatroomWindow.cs      # Chatroom creation dialog
│   ├── ChangeChatroomNameWindow.cs  # Rename chatroom dialog
│   └── ViewPinnedMessagesWindow.cs  # Pinned messages viewer
│
├── 📂 documentation-website/        # Project documentation site
├── 📂 presentation-website/         # Project presentation site
│
├── 📄 Banter.csproj                 # Project configuration
├── 📄 BanterLogo.txt                # ASCII art logo
├── 📄 Schema.txt                    # Database schema
├── 📄 LICENSE                       # MIT License
└── 📄 README.md                     # This file
```

---

## 🚀 Getting Started

### Prerequisites

- **[.NET 10.0 SDK](https://dotnet.microsoft.com/download)** or later
- **Google Cloud Project** with Firestore enabled
- **Terminal** that supports 256 colors (recommended: Windows Terminal, iTerm2, or modern Linux terminals)

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/dreeyanzz/Banter.git
   cd Banter
   ```

2. **Configure Firebase Credentials**

   The project uses an embedded Firebase Admin SDK key. To set up your own:

   - Create a Firebase project at [console.firebase.google.com](https://console.firebase.google.com)
   - Enable Firestore Database
   - Generate a service account key (JSON)
   - Update `FirestoreManager.cs`:
     ```csharp
     private const string ProjectId = "your-project-id";
     string resourceName = "Banter.your-firebase-key.json";
     ```
   - Add the JSON file to the project and set its **Build Action** to `Embedded Resource`

3. **Restore Dependencies**

   ```bash
   dotnet restore
   ```

4. **Build the Project**

   ```bash
   dotnet build
   ```

5. **Run Banter**
   ```bash
   dotnet run
   ```

---

## 🕹️ Usage Guide

### First Time Setup

1. **Launch Banter** - The login screen will appear with the ASCII logo
2. **Create an Account**:
   - Click "Create Account"
   - Enter a username (min. 8 characters)
   - Set a password (min. 8 characters)
   - Provide your name and email
3. **Login** with your new credentials

### Main Interface

Once logged in, you'll see three main windows:

```
┌─────────────┬──────────────────────────────┬─────────────┐
│   Window1   │          Window2             │   Window3   │
│  (People)   │       (Main Chat)            │ (Chat Info) │
│             │                              │             │
│ • Chatrooms │ • Chat History               │ • Settings  │
│ • Search    │ • Message Input              │ • Admin     │
│ • User Info │ • Pinned Messages            │   Controls  │
└─────────────┴──────────────────────────────┴─────────────┘
```

### Controls

- **Mouse**: Click buttons, select text, and switch between windows
- **Tab/Shift+Tab**: Navigate between UI elements
- **Enter**: Send message or activate focused button
- **Esc**: Close dialogs
- **Ctrl+Q**: Quit application (from File menu)

### Creating a Chatroom

1. Click **"+ Add Chatroom"** in Window1
2. Type usernames to add (one at a time)
3. Click **"Add"** after each username
4. Click **"Create"** when done

### Sending Messages

1. Select a chatroom from Window1
2. Type your message in the bottom text field of Window2
3. Press **Enter** or click **"Send"**

### Pinning Messages

1. In Window2, click on any message in the chat history
2. The message will be pinned (marked with a bullet •)
3. Click **"View pinned messages"** to see all pinned messages
4. Click a pinned message in the viewer to unpin it

### Admin Features (Group Chats Only)

If you're an admin of a group chatroom, Window3 will show:

- **Change chatroom name**: Rename the chatroom
- **Clear Messages**: Delete all messages (for everyone)
- **Delete Chatroom**: Permanently remove the chatroom
- **Leave Chatroom**: Remove yourself from participants

---

## 🔒 Security Considerations

⚠️ **Important Security Notes**:

1. **Passwords are stored in plaintext** in the current implementation
   - This is **NOT production-ready**
   - Implement proper password hashing (bcrypt, Argon2) before deployment
2. **No input sanitization for SQL injection** (Firestore is NoSQL, but still validate inputs)

3. **Firebase credentials are embedded** in the application

   - Use environment variables or secure vaults in production
   - Never commit credentials to public repositories

4. **Email validation is basic**
   - Consider sending verification emails for production use

---

## 🐛 Known Issues & TODOs

- [ ] Implement proper password hashing
- [ ] Add file/image sharing capabilities
- [ ] Implement emoji support
- [ ] Add direct message notifications
- [ ] Improve offline message handling
- [ ] Add message edit/delete functionality
- [ ] Implement typing indicators
- [ ] Add voice note support (if feasible in TUI)
- [ ] Create comprehensive unit tests
- [ ] Add CI/CD pipeline

---

## 🤝 Contributing

Contributions are welcome! Whether you want to add new features, fix bugs, or improve documentation, your help is appreciated.

### How to Contribute

1. **Fork** the repository
2. **Create a feature branch**
   ```bash
   git checkout -b feature/AmazingFeature
   ```
3. **Commit your changes**
   ```bash
   git commit -m 'Add some AmazingFeature'
   ```
4. **Push to the branch**
   ```bash
   git push origin feature/AmazingFeature
   ```
5. **Open a Pull Request**

### Development Guidelines

- Follow C# coding conventions
- Add XML documentation comments to public methods
- Test your changes thoroughly
- Update README if you add new features

---

## 📊 Database Schema

### Collections

**Users**

```
{
  "username": string,
  "password": string,  // ⚠️ Currently plaintext
  "name": string,
  "email": string,
  "chatrooms": string[]
}
```

**Chatrooms**

```
{
  "chatroom_name": string,
  "participants": string[],
  "admins": string[],
  "type": "group" | "individual",
  "last_chat": string,
  "pinned_messages": string[]
}
```

**Messages** (subcollection of Chatrooms)

```
{
  "sender_id": string,
  "text": string,
  "timestamp": Timestamp
}
```

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2024 dreeyanzz

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

---

## 🙏 Acknowledgments

- **[Terminal.Gui](https://github.com/gui-cs/Terminal.Gui)** - For the amazing TUI framework
- **Google Cloud Firestore** - For real-time database capabilities
- **Miguel de Icaza** - For creating Terminal.Gui
- The open-source community for continuous inspiration

---

## 📧 Contact

**Developer**: [dreeyanzz](https://github.com/dreeyanzz)  
**Project Link**: [https://github.com/dreeyanzz/Banter](https://github.com/dreeyanzz/Banter)  
**Documentation**: [https://dreeyanzz.github.io/Banter/](https://dreeyanzz.github.io/Banter/)

---

## 🌟 Star History

If you find Banter useful, please consider giving it a star ⭐ on GitHub!

---

<div align="center">

**Made with ❤️ and ☕ by dreeyanzz**

_Where modernity embraces tradition_

</div>
