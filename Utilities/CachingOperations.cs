using LiteDB;

namespace Banter.Utilities
{
    /// <summary>
    /// Handles local caching of user data using LiteDB.
    /// </summary>
    public static class LocalCacheHandler
    {
        private static readonly LiteDatabase db = new("Cache.db");
        private static readonly ILiteCollection<User> users = db.GetCollection<User>("Users");

        /// <summary>
        /// Occurs when the users cache is updated.
        /// </summary>
        public static event Action<List<User>>? UsersCacheUpdated;

        /// <summary>
        /// Caches a user's data locally. If the user already exists, their data is updated.
        /// </summary>
        /// <param name="user">The user to cache.</param>
        public static void CacheUser(User user)
        {
            users.Upsert(user);
            UsersCacheUpdated?.Invoke([.. users.FindAll()]);
        }
    }
}
