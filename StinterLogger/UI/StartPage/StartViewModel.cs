using StinterLogger.UI.Helpers;
using StinterLogger.UI.MainApp;

namespace StinterLogger.UI.StartPage
{
    public class StartViewModel : ObservableObject, IPageViewModel
    {
        public string Name { get; set; }

        public StartViewModel()
        {
            this.Name = "Start";
        }
    }
}
