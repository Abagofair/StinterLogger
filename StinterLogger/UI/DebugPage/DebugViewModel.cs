using StinterLogger.UI.Helpers;
using StinterLogger.UI.MainApp;

namespace StinterLogger.UI.DebugPage
{
    public class DebugViewModel : ObservableObject, IPageViewModel
    {
        public DebugViewModel()
        {
            this.Name = "Debug";
        }

        public string Name { get; set; }
    }
}
