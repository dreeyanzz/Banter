using Terminal.Gui;

namespace CpE261FinalProject
{
    public abstract class AbstractWindow : IViewable
    {
        public readonly View dummyView = new();
    }
}
