using StinterLogger.RaceLogging.Iracing.Debug;
using StinterLogger.RaceLogging.Iracing.IracingEventArgs;
using StinterLogger.UI.DetailedDebug;
using StinterLogger.UI.MainApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
