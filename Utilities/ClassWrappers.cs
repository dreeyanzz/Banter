using Terminal.Gui;

namespace CpE261FinalProject
{
    internal class FixedWindow : Window
    {
        public FixedWindow()
            : base() { }

        public override bool MouseEvent(MouseEvent me)
        {
            // Swallow any mouse events in the title bar (which is where dragging happens)
            if (
                me.Flags.HasFlag(MouseFlags.Button1Pressed)
                || me.Flags.HasFlag(MouseFlags.Button1Clicked)
                || me.Flags.HasFlag(MouseFlags.Button1DoubleClicked)
                || me.Flags.HasFlag(MouseFlags.ReportMousePosition)
            )
            {
                // Return true = "I handled it", so Terminal.Gui won't start dragging
                return true;
            }

            return base.MouseEvent(me);
        }
    }
}
