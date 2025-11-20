using Banter.Utilities;
using Terminal.Gui;

namespace Banter.Windows
{
    /// <summary>
    /// An abstract base class for windows in the application, implementing the IViewable interface.
    /// </summary>
    public abstract class AbstractWindow : IViewable
    {
        /// <summary>
        /// A dummy view that can be used by derived classes.
        /// </summary>
        public readonly View dummyView = new();
    }
}
