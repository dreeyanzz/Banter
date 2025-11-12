using LiteDB;

namespace CpE261FinalProject
{
    public static class LocalCacheHandler
    {
        private static readonly LiteDatabase db = new("Cache.db");
        private static readonly ILiteCollection<User> users = db.GetCollection<User>("Users");

        // private static readonly ILiteCollection<User> chatrooms = db.GetCollection<Chatroom>("Chatrooms");

        public static event Action<List<User>>? UsersCacheUpdated;

        public static void CacheUser(User user)
        {
            users.Upsert(user);
            UsersCacheUpdated?.Invoke([.. users.FindAll()]);
        }
    }
}
