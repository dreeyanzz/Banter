using Google.Cloud.Firestore;
using LiteDB;

namespace CpE261FinalProject
{
    [FirestoreData]
    public class User
    {
        [BsonIgnore]
        public ObjectId? id;

        [FirestoreProperty("email")]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty("name")]
        public string Name { get; set; } = string.Empty;

        [FirestoreProperty("password")]
        public string Password { get; set; } = string.Empty; //! Reminder: plaintext storage is unsafe!

        [FirestoreProperty("username")]
        public string Username { get; set; } = string.Empty;

        [FirestoreProperty("chatrooms")]
        public List<string> Chatrooms { get; set; } = [];

        public User(string email, string name, string password, string username)
        {
            Email = email;
            Name = name;
            Password = password;
            Username = username;
            Chatrooms = [];
        }

        // Firestore deserialization requires a parameterless constructor
        public User() { }
    }

    [FirestoreData]
    public class Chatroom
    {
        [BsonIgnore]
        public ObjectId? id;

        [FirestoreProperty("chatroom_name")]
        public string? ChatroomName { get; set; } = null;

        [FirestoreProperty("last_chat")]
        public string? LastChat { get; set; } = null;

        [FirestoreProperty("participants")]
        public List<string> Participants { get; set; } = [];

        [FirestoreProperty("type")]
        public string Type { get; set; } = string.Empty;

        [FirestoreProperty("messages")]
        public List<Message> Messages { get; set; } = [];

        [FirestoreProperty("admins")]
        public List<string> Admins { get; set; } = [];
    }

    [FirestoreData]
    public class Message
    {
        [BsonIgnore]
        public ObjectId? id;

        [FirestoreProperty("sender_id")]
        public string SenderId { get; set; } = string.Empty;

        [FirestoreProperty("text")]
        public string Text { get; set; } = string.Empty;

        [FirestoreProperty("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
