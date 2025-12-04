using Google.Cloud.Firestore;
using Terminal.Gui;

namespace Banter.Utilities
{
    /// <summary>
    /// Provides helper methods for interacting with the Firestore database.
    /// </summary>
    public static class FirebaseHelper
    {
        #region Database Instance

        /// <summary>
        /// The Firestore database instance.
        /// </summary>
        public static readonly FirestoreDb db = FirestoreManager.Instance.Database;

        #endregion

        #region User Management

        /// <summary>
        /// Adds a new user account to the database.
        /// </summary>
        public static async Task<bool> AddAccount(User user)
        {
            try
            {
                CollectionReference usersRef = db.Collection(path: "Users");
                await usersRef.AddAsync(documentData: user);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(value: "Firebase AddAccount FAILED: " + ex.Message);
                MessageBox.ErrorQuery(
                    width: 50,
                    height: 10,
                    title: "DEBUG FAILED",
                    message: ex.Message,
                    buttons: ["OK"]
                );
                return false;
            }
        }

        /// <summary>
        /// Gets the username of a user by their ID.
        /// </summary>
        public static async Task<string> GetUserName(string user_id)
        {
            Dictionary<string, object> user_info = await GetUserInfoById(user_id) ?? [];

            if (user_info.Count == 0)
                return "Unknown User";

            if (!user_info.TryGetValue("name", out object? user_name) || user_name == null)
                return "Unnamed User";

            return user_name.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Asynchronously retrieves a specific user's document data.
        /// </summary>
        public static async Task<Dictionary<string, object>> GetUserInfoById(string user_id)
        {
            CollectionReference usersRef = db.Collection(path: "Users");
            DocumentReference docRef = usersRef.Document(path: user_id);

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
                return snapshot.ToDictionary();
            else
                return [];
        }

        /// <summary>
        /// Gets the user ID associated with a given username.
        /// </summary>
        public static async Task<string> GetUserIdFromUsername(string username)
        {
            CollectionReference usersRef = db.Collection(path: "Users");
            Query query = usersRef.WhereEqualTo(fieldPath: "username", value: username);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count > 0)
            {
                DocumentSnapshot document = snapshot.Documents[0];
                return document.Id;
            }

            return string.Empty;
        }

        /// <summary>
        /// Checks if a username is already taken.
        /// </summary>
        public static async Task<bool> IsUsernameTaken(string username)
        {
            CollectionReference usersRef = db.Collection(path: "Users");
            Query query = usersRef.WhereEqualTo(fieldPath: "username", value: username);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            return !(snapshot.Count > 0);
        }

        /// <summary>
        /// Validates if a username exists in the database (Iterative approach).
        /// </summary>
        public static async Task<bool> ValidateUsername(string username)
        {
            CollectionReference usersRef = db.Collection(path: "Users");
            QuerySnapshot snapshot = await usersRef.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (
                    document.Exists
                    && document.TryGetValue(path: "username", value: out string dbUsername)
                )
                {
                    if (dbUsername == username)
                        return true;
                }
            }

            return false;
        }

        #endregion

        #region Chatroom Lifecycle (Create, Delete, Rename)

        /// <summary>
        /// Creates a new chatroom.
        /// </summary>
        public static async Task CreateChatroom(List<string> participants_ids)
        {
            if (participants_ids.Count == 0)
                return;

            if (!participants_ids.Contains(item: SessionHandler.UserId!))
                participants_ids.Add(item: SessionHandler.UserId!);

            string chatroom_type = participants_ids.Count > 2 ? "group" : "individual";

            IEnumerable<Task<string>>? nameTasks = participants_ids.Select(
                selector: FirebaseHelper.GetUserName
            );
            string[]? participants_names = await Task.WhenAll(tasks: nameTasks);
            string chatroom_name = string.Join(separator: ", ", values: participants_names);

            Chatroom chatroom_info = new()
            {
                ChatroomName = chatroom_name,
                Participants = participants_ids,
                Type = chatroom_type,
                Admins = [SessionHandler.UserId!],
            };

            await db.Collection(path: "Chatrooms").AddAsync(documentData: chatroom_info);
        }

        /// <summary>
        /// Deletes a chatroom by its ID.
        /// </summary>
        public static async Task DeleteChatroomById(string chatroom_id)
        {
            CollectionReference chatroomsRef = db.Collection(path: "Chatrooms");
            DocumentReference chatroom = chatroomsRef.Document(path: chatroom_id);
            await chatroom.DeleteAsync();
        }

        /// <summary>
        /// Changes the name of a chatroom.
        /// </summary>
        public static async Task ChangeChatroomName(string chatroom_id, string new_name)
        {
            if (string.IsNullOrWhiteSpace(value: chatroom_id))
                throw new ArgumentException(
                    "Chatroom ID cannot be null or empty.",
                    nameof(chatroom_id)
                );

            if (string.IsNullOrWhiteSpace(value: new_name))
                throw new ArgumentException(
                    "New chatroom name cannot be null or empty.",
                    nameof(new_name)
                );

            CollectionReference chatroomsRef = db.Collection(path: "Chatrooms");
            DocumentReference chatroomRef = chatroomsRef.Document(path: chatroom_id);

            DocumentSnapshot snapshot = await chatroomRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Console.WriteLine(value: $"Chatroom with ID '{chatroom_id}' does not exist.");
                return;
            }

            await chatroomRef.UpdateAsync(
                updates: new Dictionary<string, object> { { "chatroom_name", new_name } }
            );

            Console.WriteLine(value: $"Chatroom '{chatroom_id}' name updated to '{new_name}'.");
        }

        #endregion

        #region Chatroom Metadata (Name, Type)

        /// <summary>
        /// Gets the name of a chatroom by its ID.
        /// </summary>
        public static async Task<string> GetChatroomNameById(string chatroom_id)
        {
            if (string.IsNullOrEmpty(value: chatroom_id))
                return "No chatroom_id provided";

            DocumentSnapshot snapshot = await db.Collection(path: "Chatrooms")
                .Document(path: chatroom_id)
                .GetSnapshotAsync();

            if (!snapshot.Exists)
                return "Unknown Chatroom";

            string chatroom_type = snapshot.GetValue<string>(path: "type");

            if (chatroom_type == "group")
            {
                return snapshot.TryGetValue(path: "chatroom_name", value: out string name)
                    ? name
                    : "Unnamed Group";
            }

            if (snapshot.TryGetValue(path: "participants", value: out List<string> participants))
            {
                foreach (string participant in participants)
                {
                    if (participant != SessionHandler.UserId)
                    {
                        string otherName = await FirebaseHelper.GetUserName(user_id: participant);
                        return string.IsNullOrEmpty(value: otherName) ? "Unknown User" : otherName;
                    }
                }
            }

            return "Unknown Chatroom";
        }

        /// <summary>
        /// Gets the type of a chatroom by its ID.
        /// </summary>
        public static async Task<string> GetChatroomTypeById(string chatroom_id)
        {
            CollectionReference chatroomsRef = db.Collection(path: "Chatrooms");
            DocumentReference chatroom = chatroomsRef.Document(path: chatroom_id);

            DocumentSnapshot snapshot = await chatroom.GetSnapshotAsync();
            string chatroom_type = snapshot.GetValue<string>(path: "type");

            return chatroom_type;
        }

        #endregion

        #region Participants & Admins

        /// <summary>
        /// Gets a dictionary of participants in a chatroom.
        /// </summary>
        public static async Task<Dictionary<string, string>> GetChatroomParticipants(
            string chatroom_id
        )
        {
            if (string.IsNullOrEmpty(value: chatroom_id))
                return [];

            CollectionReference chatroomsRef = db.Collection(path: "Chatrooms");
            DocumentReference docRef = chatroomsRef.Document(path: chatroom_id);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            snapshot.TryGetValue(path: "participants", value: out List<string> participants);

            Dictionary<string, string> idUserMap = [];
            foreach (string participant in participants)
            {
                string name = await GetUserName(user_id: participant);
                idUserMap.Add(key: participant, value: name);
            }

            return idUserMap;
        }

        /// <summary>
        /// Removes a participant from a chatroom.
        /// </summary>
        public static async Task RemoveChatroomParticipant(
            string participant_id,
            string chatroom_id
        )
        {
            CollectionReference chatroomsRef = db.Collection(path: "Chatrooms");
            DocumentReference chatroomRef = chatroomsRef.Document(path: chatroom_id);

            await chatroomRef.UpdateAsync(
                new Dictionary<string, object>
                {
                    { "participants", FieldValue.ArrayRemove(values: participant_id) },
                }
            );
        }

        /// <summary>
        /// Gets a list of admin user IDs for a chatroom.
        /// </summary>
        public static async Task<List<string>> GetChatroomAdmins(string chatroom_id)
        {
            if (string.IsNullOrEmpty(value: chatroom_id))
                return [];

            CollectionReference chatroomsRef = db.Collection(path: "Chatrooms");
            DocumentReference chatroom = chatroomsRef.Document(path: chatroom_id);

            DocumentSnapshot snapshot = await chatroom.GetSnapshotAsync();
            List<string> admins = snapshot.GetValue<List<string>>(path: "admins");

            return admins;
        }

        /// <summary>
        /// Validates if a user is an admin of the current chatroom.
        /// </summary>
        public static async Task<bool> ValidateChatroomAdmin(string user_id)
        {
            if (string.IsNullOrEmpty(value: user_id))
                return false;

            List<string> admins =
                await GetChatroomAdmins(chatroom_id: SessionHandler.CurrentChatroomId!) ?? [];

            if (admins.Count == 0)
                return false;

            return admins.Contains(item: user_id);
        }

        #endregion

        #region Messages

        /// <summary>
        /// Gets the content of a specific message in a chatroom.
        /// </summary>
        public static async Task<string> GetChatroomMessageById(
            string chatroom_id,
            string message_id
        )
        {
            if (string.IsNullOrEmpty(value: message_id))
                return string.Empty;

            DocumentReference messageRef = db.Collection(path: "Chatrooms")
                .Document(path: chatroom_id)
                .Collection(path: "messages")
                .Document(path: message_id);

            DocumentSnapshot snapshot = await messageRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return string.Empty;

            if (snapshot.TryGetValue(path: "text", value: out string messageContent))
                return messageContent;

            return string.Empty;
        }

        /// <summary>
        /// Clears all messages from a chatroom.
        /// </summary>
        public static async Task ClearChatroomMessagesById(string chatroom_id)
        {
            if (string.IsNullOrEmpty(chatroom_id))
                return;

            CollectionReference chatroomsRef = db.Collection(path: "Chatrooms");
            DocumentReference chatroom = chatroomsRef.Document(path: chatroom_id);
            CollectionReference messagesRef = chatroom.Collection(path: "messages");

            QuerySnapshot snapshot = await messagesRef.GetSnapshotAsync();

            if (snapshot.Count == 0)
                return;

            WriteBatch batch = db.StartBatch();

            foreach (DocumentSnapshot doc in snapshot.Documents)
                batch.Delete(documentReference: doc.Reference);

            await batch.CommitAsync();
        }

        #endregion

        #region Pinned Messages

        /// <summary>
        /// Pins a message in a chatroom.
        /// </summary>
        public static async Task PinChatroomMessage(string chatroom_id, string message_id)
        {
            if (string.IsNullOrEmpty(value: chatroom_id) || string.IsNullOrEmpty(value: message_id))
                return;

            DocumentReference chatroomRef = db.Collection(path: "Chatrooms")
                .Document(path: chatroom_id);
            await chatroomRef.UpdateAsync(
                field: "pinned_messages",
                value: FieldValue.ArrayUnion(values: message_id)
            );
        }

        /// <summary>
        /// Removes a pinned message from a chatroom.
        /// </summary>
        public static async Task RemovePinChatroomMessage(string chatroom_id, string message_id)
        {
            bool isValid =
                string.IsNullOrEmpty(value: message_id) || string.IsNullOrEmpty(value: chatroom_id);
            if (isValid)
                return;

            CollectionReference chatroomsRef = db.Collection(path: "Chatrooms");
            DocumentReference chatroomRef = chatroomsRef.Document(path: chatroom_id);
            await chatroomRef.UpdateAsync(
                field: "pinned_messages",
                value: FieldValue.ArrayRemove(values: message_id)
            );
        }

        /// <summary>
        /// Gets a list of pinned message IDs for a chatroom.
        /// </summary>
        public static async Task<List<string>> GetChatroomPinnedMessagesIdById(string chatroom_id)
        {
            if (string.IsNullOrEmpty(value: chatroom_id))
                return [];

            DocumentReference chatroomRef = db.Collection(path: "Chatrooms")
                .Document(path: chatroom_id);
            DocumentSnapshot snapshot = await chatroomRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return [];

            if (
                !snapshot.TryGetValue(
                    path: "pinned_messages",
                    value: out List<string> pinnedMessagesId
                )
            )
                return [];

            return pinnedMessagesId;
        }

        /// <summary>
        /// Checks if a message is pinned in a chatroom.
        /// </summary>
        public static async Task<bool> IsChatPinnedById(string chatroom_id, string message_id)
        {
            if (string.IsNullOrEmpty(value: chatroom_id) || string.IsNullOrEmpty(value: message_id))
                return false;

            List<string> pinnedMessageIds = await FirebaseHelper.GetChatroomPinnedMessagesIdById(
                chatroom_id: chatroom_id
            );

            if (!pinnedMessageIds.Contains(item: message_id))
                return false;

            return true;
        }

        #endregion
    }
}
