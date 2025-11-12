using System;
using System.Reflection;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace CpE261FinalProject
{
    public sealed class FirestoreManager
    {
        private static readonly Lazy<FirestoreManager> lazyInstance = new(() =>
            new FirestoreManager()
        );

        public static FirestoreManager Instance => lazyInstance.Value;

        // The actual Firestore database object
        public FirestoreDb Database { get; private set; }

        //  Google Cloud Project ID
        private const string ProjectId = "banter-7717f";

        private FirestoreManager()
        {
            try
            {
                Console.WriteLine("Initializing Firestore connection...");

                Assembly assembly = Assembly.GetExecutingAssembly();

                string resourceName =
                    "CpE261FinalProject.banter-7717f-firebase-adminsdk-fbsvc-9cb1cd3095.json";

                // 5. Load the file as a Stream
                GoogleCredential credential;
                using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new Exception(
                            $"Error: Could not find the embedded JSON key. Make sure the name is '{resourceName}' and its Build Action is 'Embedded Resource'."
                        );
                    }

                    credential = GoogleCredential.FromStream(stream);
                }

                Database = new FirestoreDbBuilder
                {
                    ProjectId = ProjectId,
                    Credential = credential,
                }.Build();

                Console.WriteLine("Firestore connection successful (standalone mode).");
            }
            catch (Exception ex)
            {
                // Handle initialization errors
                Console.WriteLine($"Error initializing Firestore: {ex.Message}");
                Database = null!;
            }
        }
    }
}
