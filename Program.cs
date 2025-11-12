using Google.Cloud.Firestore;
using Terminal.Gui;

namespace CpE261FinalProject
{
    internal class Program
    {
        private static void Main()
        {
            Application.Init();

            List<MenuItem> fileMenuItems =
            [
                new("About", "", () => MessageBox.Query("About", "This is Banter!", "Ok")),
                new("Quit", "", () => Application.RequestStop()),
            ];

            List<MenuBarItem> menuBarItems = [new("File", [.. fileMenuItems])];

            MenuBar menuBar = new([.. menuBarItems]);

            Application.Top.Add(views: [menuBar]);

            LogInWindow.Instance.Show();

            // Window1.Instance.Show();
            // Window2.Instance.Show();
            // Window3.Instance.Show();

            Application.Run();
            Application.Shutdown();
        }
    };
}
