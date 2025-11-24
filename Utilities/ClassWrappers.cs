using Terminal.Gui;

namespace Banter.Utilities
{
    /// <summary>
    /// A custom Window class that prevents dragging.
    /// </summary>
    internal class FixedWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FixedWindow"/> class.
        /// </summary>
        public FixedWindow()
            : base() { }

        /// <summary>
        /// Overrides the default mouse event handling to prevent window dragging.
        /// </summary>
        /// <param name="me">The mouse event to handle.</param>
        /// <returns><c>true</c> if the event was handled; otherwise, <c>false</c>.</returns>
        public override bool MouseEvent(MouseEvent me)
        {
            // Swallow any mouse events in the title bar (which is where dragging happens)
            if (
                me.Flags.HasFlag(flag: MouseFlags.Button1Pressed)
                || me.Flags.HasFlag(flag: MouseFlags.Button1Clicked)
                || me.Flags.HasFlag(flag: MouseFlags.Button1DoubleClicked)
                || me.Flags.HasFlag(flag: MouseFlags.ReportMousePosition)
            )
            {
                // Return true = "I handled it", so Terminal.Gui won't start dragging
                return true;
            }

            return base.MouseEvent(mouseEvent: me);
        }
    }
}
