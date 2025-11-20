using Terminal.Gui;

namespace Banter.Utilities
{
    /// <summary>
    /// Provides helper methods for managing windows in Terminal.Gui.
    /// </summary>
    public static class WindowHelper
    {
        /// <summary>
        /// Focuses a window, adding it to the top-level view if it's not already present.
        /// </summary>
        /// <param name="window">The window to focus.</param>
        public static void FocusWindow(Window window)
        {
            if (!Application.Top.Subviews.Contains(window))
                Application.Top.Add(view: window);
            window.SetFocus();
        }

        /// <summary>
        /// Adds a window to the top-level view.
        /// </summary>
        /// <param name="window">The window to open.</param>
        public static void OpenWindow(Window window)
        {
            Application.Top.Add(view: window);
        }

        /// <summary>
        /// Removes a window from the top-level view.
        /// </summary>
        /// <param name="window">The window to close.</param>
        public static void CloseWindow(Window window)
        {
            Application.Top.Remove(view: window);
        }

        /// <summary>
        /// Closes all open windows.
        /// </summary>
        public static void CloseAllWindows()
        {
            List<Window> windows = [.. Application.Top.Subviews.OfType<Window>()];

            foreach (Window window in windows)
                CloseWindow(window);
        }

        /// <summary>
        /// Calculates the horizontal distance between two windows.
        /// </summary>
        /// <param name="left">The left window.</param>
        /// <param name="right">The right window.</param>
        /// <returns>The horizontal distance between the two windows.</returns>
        public static int GetDistanceX(Window left, Window right)
        {
            int initial = left.Frame.X + left.Frame.Width;
            int final = right.Frame.X;

            return final - initial;
        }
    }
}
