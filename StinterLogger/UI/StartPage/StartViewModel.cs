using StinterLogger.UI.Helpers;
using StinterLogger.UI.MainApp;

namespace StinterLogger.UI.StartPage
{
    public class StartViewModel : ObservableObject, IPageViewModel
    {
        public StartViewModel()
        {
            this.Name = "Start";
        }

        public string Name { get; set; }
    }
}
