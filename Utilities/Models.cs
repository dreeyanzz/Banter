using Google.Cloud.Firestore;
using LiteDB;

namespace Banter.Utilities
{
    /// <summary>
    /// Represents a user in the application.
    /// </summary>
    [FirestoreData]
    public class User
    {
        /// <summary>
        /// The local database ID.
        /// </summary>
        [BsonIgnore]
        public ObjectId? id;

        /// <summary>
        /// The user's email address.
        /// </summary>
        [FirestoreProperty("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The user's name.
        /// </summary>
        [FirestoreProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The user's password.
        /// </summary>
        [FirestoreProperty("password")]
        public string Password { get; set; } = string.Empty; //! Reminder: plaintext storage is unsafe!

        /// <summary>
        /// The user's username.
        /// </summary>
        [FirestoreProperty("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// A list of chatroom IDs the user is a member of.
        /// </summary>
        [FirestoreProperty("chatrooms")]
        public List<string> Chatrooms { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="email">The user's email.</param>
        /// <param name="name">The user's name.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="username">The user's username.</param>
        public User(string email, string name, string password, string username)
        {
            Email = email;
            Name = name;
            Password = password;
            Username = username;
            Chatrooms = [];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        // Firestore deserialization requires a parameterless constructor
        public User() { }
    }

    /// <summary>
    /// Represents a chatroom in the application.
    /// </summary>
    [FirestoreData]
    public class Chatroom
    {
        /// <summary>
        /// The local database ID.
        /// </summary>
        [BsonIgnore]
        public ObjectId? id;

        /// <summary>
        /// The name of the chatroom.
        /// </summary>
        [FirestoreProperty("chatroom_name")]
        public string? ChatroomName { get; set; } = null;

        /// <summary>
        /// The last message sent in the chatroom.
        /// </summary>
        [FirestoreProperty("last_chat")]
        public string? LastChat { get; set; } = null;

        /// <summary>
        /// A list of participant user IDs.
        /// </summary>
        [FirestoreProperty("participants")]
        public List<string> Participants { get; set; } = [];

        /// <summary>
        /// The type of chatroom (e.g., "group" or "individual").
        /// </summary>
        [FirestoreProperty("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// A list of messages in the chatroom.
        /// </summary>
        [FirestoreProperty("messages")]
        public List<Message> Messages { get; set; } = [];

        /// <summary>
        /// A list of admin user IDs.
        /// </summary>
        [FirestoreProperty("admins")]
        public List<string> Admins { get; set; } = [];
    }

    /// <summary>
    /// Represents a message in a chatroom.
    /// </summary>
    [FirestoreData]
    public class Message
    {
        /// <summary>
        /// The local database ID.
        /// </summary>
        [BsonIgnore]
        public ObjectId? id;

        /// <summary>
        /// The ID of the user who sent the message.
        /// </summary>
        [FirestoreProperty("sender_id")]
        public string SenderId { get; set; } = string.Empty;

        /// <summary>
        /// The text content of the message.
        /// </summary>
        [FirestoreProperty("text")]
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the message was sent.
        /// </summary>
        [FirestoreProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
