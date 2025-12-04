using System.Diagnostics;
using Banter.Windows;
using Terminal.Gui;

namespace Banter
{
    /// <summary>
    /// The main class of the Banter application.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            Application.Init();

            List<MenuItem> fileMenuItems =
            [
                new("About", "", () => MessageBox.Query("About", "This is Banter!", "Ok")),
                new(
                    "Class Diagram",
                    "",
                    () =>
                    {
                        Process.Start(
                            new ProcessStartInfo
                            {
                                FileName = "Banter_Class_Diagram.png",
                                UseShellExecute = true,
                            }
                        );
                    }
                ),
                new(
                    "Help",
                    "",
                    () =>
                        MessageBox.Query(
                            "Help",
                            "Before you use this app, make sure you have strong internet.\nDo not forget that you can use both Mouse and Keyboard!",
                            "Confirm"
                        )
                ),
                new("Quit", "", () => Application.RequestStop()),
            ];

            List<MenuBarItem> menuBarItems = [new("File", [.. fileMenuItems])];

            MenuBar menuBar = new([.. menuBarItems]);

            Application.Top.Add(views: [menuBar]);

            LogInWindow.Instance.Show();

            Application.Run();
            Application.Shutdown();
        }
    };
}
