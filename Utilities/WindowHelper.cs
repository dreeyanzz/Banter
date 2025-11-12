using Terminal.Gui;

namespace CpE261FinalProject
{
    public static class WindowHelper
    {
        public static void FocusWindow(Window window)
        {
            if (!Application.Top.Subviews.Contains(window))
                Application.Top.Add(view: window);
            window.SetFocus();
        }

        public static void OpenWindow(Window window)
        {
            Application.Top.Add(view: window);
        }

        public static void CloseWindow(Window window)
        {
            Application.Top.Remove(view: window);
        }

        public static void CloseAllWindows()
        {
            List<Window> windows = [.. Application.Top.Subviews.OfType<Window>()];

            foreach (Window window in windows)
                CloseWindow(window);
        }

        public static int GetDistanceX(Window left, Window right)
        {
            int initial = left.Frame.X + left.Frame.Width;
            int final = right.Frame.X;

            return final - initial;
        }
    }
}
