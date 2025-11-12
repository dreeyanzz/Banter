using Google.Cloud.Firestore;
using Terminal.Gui;

namespace CpE261FinalProject
{
    public static class FirebaseHelper
    {
        public static FirestoreDb db = FirestoreManager.Instance.Database;

        public static async Task ChangeChatroomName(string chatroom_id, string new_name)
        {
            if (string.IsNullOrWhiteSpace(chatroom_id))
                throw new ArgumentException(
                    "Chatroom ID cannot be null or empty.",
                    nameof(chatroom_id)
                );

            if (string.IsNullOrWhiteSpace(new_name))
                throw new ArgumentException(
                    "New chatroom name cannot be null or empty.",
                    nameof(new_name)
                );

            CollectionReference chatroomsRef = db.Collection("Chatrooms");
            DocumentReference chatroomRef = chatroomsRef.Document(chatroom_id);

            // Fetch the document snapshot first to check if it exists
            DocumentSnapshot snapshot = await chatroomRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                Console.WriteLine($"Chatroom with ID '{chatroom_id}' does not exist.");
                return; // Or throw a custom exception if you prefer
            }

            // Update the chatroom_name safely
            await chatroomRef.UpdateAsync(
                new Dictionary<string, object> { { "chatroom_name", new_name } }
            );

            Console.WriteLine($"Chatroom '{chatroom_id}' name updated to '{new_name}'.");
        }

        public static async Task RemoveChatroomParticipant(
            string participant_id,
            string chatroom_id
        )
        {
            CollectionReference chatroomsRef = db.Collection("Chatrooms");
            DocumentReference chatroomRef = chatroomsRef.Document(chatroom_id);

            // Remove the participant from the array
            await chatroomRef.UpdateAsync(
                new Dictionary<string, object>
                {
                    { "participants", FieldValue.ArrayRemove(participant_id) },
                }
            );
        }

        public static async Task<bool> ValidateChatroomAdmin(string user_id)
        {
            List<string> admins = await GetChatroomAdmins(SessionHandler.CurrentChatroomId!);

            return admins.Contains(user_id);
        }

        public static async Task<List<string>> GetChatroomAdmins(string chatroom_id)
        {
            CollectionReference chatroomsRef = db.Collection("Chatrooms");
            DocumentReference chatroom = chatroomsRef.Document(SessionHandler.CurrentChatroomId);

            DocumentSnapshot snapshot = await chatroom.GetSnapshotAsync();

            List<string> admins = snapshot.GetValue<List<string>>("admins");

            return admins;
        }

        public static async Task CreateChatroom(List<string> participants_ids)
        {
            if (participants_ids == null || participants_ids.Count == 0)
                return;

            // Always include the current user
            if (!participants_ids.Contains(SessionHandler.UserId!))
                participants_ids.Add(SessionHandler.UserId!);

            string chatroom_type = participants_ids.Count > 2 ? "group" : "individual";

            // Fetch names concurrently
            IEnumerable<Task<string>>? nameTasks = participants_ids.Select(
                FirebaseHelper.GetUserName
            );
            string[]? participants_names = await Task.WhenAll(nameTasks);

            // Build readable name
            string chatroom_name = string.Join(", ", participants_names);

            Chatroom chatroom_info = new()
            {
                ChatroomName = chatroom_name,
                Participants = participants_ids,
                Type = chatroom_type,
                Admins = [SessionHandler.UserId!],
            };

            await db.Collection("Chatrooms").AddAsync(chatroom_info);
        }

        public static async Task DeleteChatroomById(string chatroom_id)
        {
            CollectionReference chatroomsRef = db.Collection("Chatrooms");
            DocumentReference chatroom = chatroomsRef.Document(chatroom_id);

            await chatroom.DeleteAsync();
        }

        public static async Task<string> GetChatroomTypeById(string chatroom_id)
        {
            CollectionReference chatroomsRef = db.Collection("Chatrooms");
            DocumentReference chatroom = chatroomsRef.Document(chatroom_id);

            DocumentSnapshot snapshot = await chatroom.GetSnapshotAsync();
            string chatroom_type = snapshot.GetValue<string>("type");

            return chatroom_type;
        }

        public static async Task ClearChatroomMessagesById(string chatroom_id)
        {
            CollectionReference chatroomsRef = db.Collection("Chatrooms");
            DocumentReference chatroom = chatroomsRef.Document(chatroom_id);
            CollectionReference messagesRef = chatroom.Collection("messages");

            QuerySnapshot snapshot = await messagesRef.GetSnapshotAsync();

            if (snapshot.Count == 0)
                return;

            WriteBatch batch = db.StartBatch();

            foreach (DocumentSnapshot doc in snapshot.Documents)
                batch.Delete(doc.Reference);

            await batch.CommitAsync();
        }

        public static async Task<string> GetChatroomNameById(string chatroom_id)
        {
            DocumentSnapshot snapshot = await db.Collection("Chatrooms")
                .Document(chatroom_id)
                .GetSnapshotAsync();

            if (!snapshot.Exists)
                return "Unknown Chatroom";

            string chatroom_type = snapshot.GetValue<string>("type");

            if (chatroom_type == "group")
            {
                return snapshot.TryGetValue("chatroom_name", out string name)
                    ? name
                    : "Unnamed Group";
            }

            // Individual chat
            if (snapshot.TryGetValue("participants", out List<string> participants))
            {
                foreach (string participant in participants)
                {
                    if (participant != SessionHandler.UserId)
                    {
                        string otherName = await FirebaseHelper.GetUserName(participant);
                        return string.IsNullOrEmpty(otherName) ? "Unknown User" : otherName;
                    }
                }
            }

            return "Unknown Chatroom";
        }

        public static async Task<Dictionary<string, string>> GetChatroomParticipants(
            string chatroom_id
        )
        {
            CollectionReference chatroomsRef = db.Collection("Chatrooms");
            DocumentReference docRef = chatroomsRef.Document(chatroom_id);

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            snapshot.TryGetValue("participants", out List<string> participants);

            Dictionary<string, string> idUserMap = [];

            foreach (string participant in participants)
            {
                string name = await GetUserName(participant);
                idUserMap.Add(participant, name);
            }

            return idUserMap;
        }

        public static async Task<string> GetUserName(string user_id)
        {
            Dictionary<string, object>? user_info = await GetUserInfoById(user_id);

            if (user_info == null)
                return "Unknown User"; // handle missing user gracefully

            if (!user_info.TryGetValue("name", out object? user_name) || user_name == null)
                return "Unnamed User"; // handle missing name

            return user_name.ToString()!;
        }

        /// <summary>
        /// Asynchronously retrieves a specific user's document data from the Firestore "Users" collection.
        /// </summary>
        /// <param name="user_id">The unique identifier (document ID) for the user to retrieve.</param>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> containing the user's document data (field names and values) if the user is found.
        /// Returns <c>null</c> if no document exists with the specified <paramref name="user_id"/>.
        /// </returns>
        public static async Task<Dictionary<string, object>?> GetUserInfoById(string user_id)
        {
            CollectionReference usersRef = db.Collection("Users");
            DocumentReference docRef = usersRef.Document(user_id);

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
                return snapshot.ToDictionary();
            else
                return null;
        }

        public static async Task<string?> GetUserIdFromUsername(string username)
        {
            CollectionReference usersRef = db.Collection("Users");

            // 1. Create a query to find the matching username
            Query query = usersRef.WhereEqualTo("username", username);

            // 2. Execute the query
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count > 0)
            {
                DocumentSnapshot document = snapshot.Documents[0];
                return document.Id;
            }

            return null;
        }

        public static async Task<bool> ValidateUsername(string username)
        {
            CollectionReference usersRef = db.Collection("Users");
            QuerySnapshot snapshot = await usersRef.GetSnapshotAsync();

            foreach (DocumentSnapshot document in snapshot.Documents)
                if (document.Exists && document.TryGetValue("username", out string dbUsername))
                    if (dbUsername == username)
                        return true;

            return false;
        }

        public static async Task<bool> IsUsernameTaken(string username)
        {
            CollectionReference usersRef = db.Collection("Users");

            Query query = usersRef.WhereEqualTo("username", username);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            return !(snapshot.Count > 0);
        }

        public static async Task<bool> AddAccount(User user)
        {
            try
            {
                CollectionReference usersRef = db.Collection("Users");
                await usersRef.AddAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                // 🚨 IMPORTANT: Log the error message to your console or log file
                // This will tell you if it's a PermissionDenied, NetworkError, etc.
                Console.WriteLine("Firebase AddAccount FAILED: " + ex.Message);

                // If possible, show a very specific error during debug
                MessageBox.ErrorQuery(50, 10, "DEBUG FAILED", ex.Message, "OK");

                return false;
            }
        }
    }
}
