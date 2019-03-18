using StinterLogger.UI.MainApp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StinterLogger.UI.DebugPage
{
    /// <summary>
    /// Interaction logic for DebugControl.xaml
    /// </summary>
    public partial class DebugControl : UserControl
    {
        public DebugControl()
        {
            InitializeComponent();
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var source = e.Source as ListBoxItem;
            var index = (source.Content as DebugModel).Index;
            var detailedLog = ((App)Application.Current).DebugLogger.GetDetailedLog(index);
            ((App)Application.Current).OpenDetailedDebugWindow(detailedLog);
        }
    }
}
