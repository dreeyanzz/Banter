using Terminal.Gui;

namespace Banter
{
    public static class CustomColorScheme
    {
        public static readonly ColorScheme Window = new()
        {
            Normal = new Terminal.Gui.Attribute(foreground: Color.Gray, background: Color.Black),

            Focus = new Terminal.Gui.Attribute(foreground: Color.White, background: Color.DarkGray),

            // These are often the same as Normal and Focus but can be customized
            HotNormal = new Terminal.Gui.Attribute(
                foreground: Color.Magenta, // Hotkeys on the window title will be Bright Cyan
                background: Color.Black
            ),

            HotFocus = new Terminal.Gui.Attribute(
                foreground: Color.Magenta,
                background: Color.DarkGray
            ),
        };

        public static readonly ColorScheme Button = new()
        {
            // Default, unfocused button state
            Normal = new Terminal.Gui.Attribute(
                foreground: Color.DarkGray,
                background: Color.Black
            ),

            // **FIX:** When unfocused, the hotkey character will now be green
            HotNormal = new Terminal.Gui.Attribute(
                foreground: Color.Green,
                background: Color.Black
            ),

            // **IMPROVEMENT:** Invert colors for a clear focus indicator
            Focus = new Terminal.Gui.Attribute(foreground: Color.Black, background: Color.Green),

            // When focused, the hotkey will use the same scheme
            HotFocus = new Terminal.Gui.Attribute(foreground: Color.Black, background: Color.Green),
        };

        public static readonly ColorScheme LabelEmpty = new()
        {
            // A Label doesn't really have different states,
            // so we make them all the same.
            // We use DarkGray to make it less prominent.
            Normal = new Terminal.Gui.Attribute(
                foreground: Color.DarkGray,
                background: Color.Black
            ),

            Focus = new Terminal.Gui.Attribute(foreground: Color.DarkGray, background: Color.Black),

            HotNormal = new Terminal.Gui.Attribute(
                foreground: Color.DarkGray,
                background: Color.Black
            ),

            HotFocus = new Terminal.Gui.Attribute(
                foreground: Color.DarkGray,
                background: Color.Black
            ),
        };
    }
}
