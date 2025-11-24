using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Terminal.Gui;

namespace Banter.Utilities
{
    /// <summary>
    /// Provides helper methods for interacting with the Firestore database.
    /// </summary>
    public static class FirebaseHelper
    {
        /// <summary>
        /// The Firestore database instance.
        /// </summary>
        public static readonly FirestoreDb db = FirestoreManager.Instance.Database;

        /// <summary>
        /// Gets the content of a specific message in a chatroom.
        /// </summary>
        /// <param name="chatroom_id">The ID of the chatroom.</param>
        /// <param name="message_id">The ID of the message.</param>
        /// <returns>The message content, or null if not found.</returns>
        public static async Task<string> GetChatroomMessageById(
            string chatroom_id,
            string message_id
        )
        {
            if (string.IsNullOrEmpty(value: message_id))
                return string.Empty;

            // Reference to the messages subcollection
            DocumentReference messageRef = db.Collection(path: "Chatrooms")
                .Document(path: chatroom_id)
                .Collection(path: "messages")
                .Document(path: message_id);

            // Get the document snapshot
            DocumentSnapshot snapshot = await messageRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return string.Empty; // message not found

            // Assuming the message content is stored in a field called "content"
            if (snapshot.TryGetValue(path: "text", value: out string messageContent))
                return messageContent;

            return string.Empty; // field not found
        }

        /// <summary>
        /// Gets a list of pinned message IDs for a chatroom.
        /// </summary>
        /// <param name="chatroom_id">The ID of the chatroom.</param>
        /// <returns>A list of pinned message IDs.</returns>
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
        /// <param name="chatroom_id">The ID of the chatroom.</param>
        /// <param name="message_id">The ID of the message.</param>
        /// <returns><c>true</c> if the message is pinned; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Pins a message in a chatroom.
        /// </summary>
        /// <param name="chatroom_id">The ID of the chatroom.</param>
        /// <param name="message_id">The ID of the message to pin.</param>
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
        /// <param name="chatroom_id">The ID of the chatroom.</param>
        /// <param name="message_id">The ID of the message to unpin.</param>
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
        /// Changes the name of a chatroom.
        /// </summary>
        /// <param name="chatroom_id">The ID of the chatroom.</param>
        /// <param name="new_name">The new name for the chatroom.</param>
        public static async Task ChangeChatroomName(string chatroom_id, string new_name)
        {
            if (string.IsNullOrWhiteSpace(value: chatroom_id))
                throw new ArgumentException(
                    message: "Chatroom ID cannot be null or empty.",
                    paramName: nameof(chatroom_id)
                );

            if (string.IsNullOrWhiteSpace(value: new_name))
                throw new ArgumentException(
                    message: "New chatroom name cannot be null or empty.",
                    paramName: nameof(new_name)
                );

            CollectionReference chatroomsRef = db.Collection(path: "Chatrooms");
            DocumentReference chatroomRef = chatroomsRef.Document(path: chatroom_id);

            // Fetch the document snapshot first to check if it exists (Chatroom)
            DocumentSnapshot snapshot = await chatroomRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Console.WriteLine(value: $"Chatroom with ID '{chatroom_id}' does not exist.");
                return; // Or throw a custom exception if you prefer
            }

            // Update the chatroom_name safely
            await chatroomRef.UpdateAsync(
                updates: new Dictionary<string, object> { { "chatroom_name", new_name } }
            );

            Console.WriteLine(value: $"Chatroom '{chatroom_id}' name updated to '{new_name}'.");
        }

        /// <summary>
        /// Removes a participant from a chatroom.
        /// </summary>
        /// <param name="participant_id">The ID of the participant to remove.</param>
        /// <param name="chatroom_id">The ID of the chatroom.</param>
        public static async Task RemoveChatroomParticipant(
            string participant_id,
            string chatroom_id
        )
        {
            CollectionReference chatroomsRef = db.Collection(path: "Chatrooms");
            DocumentReference chatroomRef = chatroomsRef.Document(path: chatroom_id);

            // Remove the participant from the array
            await chatroomRef.UpdateAsync(
                new Dictionary<string, object>
                {
                    { "participants", FieldValue.ArrayRemove(values: participant_id) },
                }
            );
        }

        /// <summary>
        /// Validates if a user is an admin of the current chatroom.
        /// </summary>
        /// <param name="user_id">The ID of the user.</param>
        /// <returns><c>true</c> if the user is an admin; otherwise, <c>false</c>.</returns>
        public static async Task<bool> ValidateChatroomAdmin(string user_id)
        {
            if (string.IsNullOrEmpty(value: user_id))
                return false;

            List<string> admins =
                await GetChatroomAdmins(
                    chatroom_id: SessionHandler.CurrentChatroomId! //! Using `!` here
                ) ?? [];
            if (admins.Count == 0)
                return false;

            return admins.Contains(item: user_id);
        }

        /// <summary>
        /// Gets a list of admin user IDs for a chatroom.
        /// </summary>
        /// <param name="chatroom_id">The ID of the chatroom.</param>
        /// <returns>A list of admin user IDs, or null if the chatroom is not found.</returns>
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
        /// Creates a new chatroom.
        /// </summary>
        /// <param name="participants_ids">A list of participant user IDs to include in the chatroom.</param>
        public static async Task CreateChatroom(List<string> participants_ids)
        {
            if (participants_ids.Count == 0)
                return;

            // Always include the current user
            if (!participants_ids.Contains(item: SessionHandler.UserId!)) //! Using `!` here
                participants_ids.Add(item: SessionHandler.UserId!);

            string chatroom_type = participants_ids.Count > 2 ? "group" : "individual";

            // Fetch names concurrently
            IEnumerable<Task<string>>? nameTasks = participants_ids.Select(
                selector: FirebaseHelper.GetUserName
            );
            string[]? participants_names = await Task.WhenAll(tasks: nameTasks);

            // Build readable name
            string chatroom_name = string.Join(separator: ", ", values: participants_names);

            Chatroom chatroom_info = new()
            {
                ChatroomName = chatroom_name,
                Participants = participants_ids,
                Type = chatroom_type,
                Admins = [SessionHandler.UserId!], //! Using `!` here
            };

            await db.Collection(path: "Chatrooms").AddAsync(documentData: chatroom_info);
        }

        /// <summary>
        /// Deletes a chatroom by its ID.
        /// </summary>
        /// <param name="chatroom_id">The ID of the chatroom to delete.</param>
        public static async Task DeleteChatroomById(string chatroom_id)
        {
            CollectionReference chatroomsRef = db.Collection(path: "Chatrooms");
            DocumentReference chatroom = chatroomsRef.Document(path: chatroom_id);

            await chatroom.DeleteAsync();
        }

        /// <summary>
        /// Gets the type of a chatroom by its ID.
        /// </summary>
        /// <param name="chatroom_id">The ID of the chatroom.</param>
        /// <returns>The chatroom type (e.g., "group" or "individual").</returns>
        public static async Task<string> GetChatroomTypeById(string chatroom_id)
        {
            CollectionReference chatroomsRef = db.Collection(path: "Chatrooms");
            DocumentReference chatroom = chatroomsRef.Document(path: chatroom_id);

            DocumentSnapshot snapshot = await chatroom.GetSnapshotAsync();
            string chatroom_type = snapshot.GetValue<string>(path: "type");

            return chatroom_type;
        }

        /// <summary>
        /// Clears all messages from a chatroom.
        /// </summary>
        /// <param name="chatroom_id">The ID of the chatroom to clear.</param>
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

        /// <summary>
        /// Gets the name of a chatroom by its ID.
        /// </summary>
        /// <param name="chatroom_id">The ID of the chatroom.</param>
        /// <returns>The name of the chatroom.</returns>
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

            // Individual chat
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
        /// Gets a dictionary of participants in a chatroom.
        /// </summary>
        /// <param name="chatroom_id">The ID of the chatroom.</param>
        /// <returns>A dictionary mapping participant IDs to their usernames, or null if the chatroom is not found.</returns>
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
        /// Gets the username of a user by their ID.
        /// </summary>
        /// <param name="user_id">The ID of the user.</param>
        /// <returns>The username of the user.</returns>
        public static async Task<string> GetUserName(string user_id)
        {
            //? Clear these things

            Dictionary<string, object> user_info = await GetUserInfoById(user_id) ?? [];

            if (user_info.Count == 0)
                return "Unknown User"; // handle missing user gracefully

            if (!user_info.TryGetValue("name", out object? user_name) || user_name == null)
                return "Unnamed User"; // handle missing name

            return user_name.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Asynchronously retrieves a specific user's document data from the Firestore "Users" collection.
        /// </summary>
        /// <param name="user_id">The unique identifier (document ID) for the user to retrieve.</param>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> containing the user's document data (field names and values) if the user is found.
        /// Returns <c>null</c> if no document exists with the specified <paramref name="user_id"/>.
        /// </returns>
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
        /// <param name="username">The username to look up.</param>
        /// <returns>The user ID, or null if the username is not found.</returns>
        public static async Task<string> GetUserIdFromUsername(string username)
        {
            CollectionReference usersRef = db.Collection(path: "Users");

            // 1. Create a query to find the matching username
            Query query = usersRef.WhereEqualTo(fieldPath: "username", value: username);

            // 2. Execute the query
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count > 0)
            {
                DocumentSnapshot document = snapshot.Documents[0];
                return document.Id;
            }

            return string.Empty;
        }

        /// <summary>
        /// Validates if a username exists in the database.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <returns><c>true</c> if the username exists; otherwise, <c>false</c>.</returns>
        public static async Task<bool> ValidateUsername(string username)
        {
            CollectionReference usersRef = db.Collection(path: "Users");
            QuerySnapshot snapshot = await usersRef.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
                if (
                    document.Exists
                    && document.TryGetValue(path: "username", value: out string dbUsername)
                )
                    if (dbUsername == username)
                        return true;

            return false;
        }

        /// <summary>
        /// Checks if a username is already taken.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <returns><c>true</c> if the username is not taken; otherwise, <c>false</c>.</returns>
        public static async Task<bool> IsUsernameTaken(string username)
        {
            CollectionReference usersRef = db.Collection(path: "Users");

            Query query = usersRef.WhereEqualTo(fieldPath: "username", value: username);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            return !(snapshot.Count > 0);
        }

        /// <summary>
        /// Adds a new user account to the database.
        /// </summary>
        /// <param name="user">The user object to add.</param>
        /// <returns><c>true</c> if the account was added successfully; otherwise, <c>false</c>.</returns>
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
                // 🚨 IMPORTANT: Log the error message to your console or log file
                // This will tell you if it's a PermissionDenied, NetworkError, etc.
                Console.WriteLine(value: "Firebase AddAccount FAILED: " + ex.Message);

                // If possible, show a very specific error during debug
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
    }
}
