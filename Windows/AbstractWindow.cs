using Terminal.Gui;

namespace Banter
{
    public abstract class AbstractWindow : IViewable
    {
        public readonly View dummyView = new();
    }
}
