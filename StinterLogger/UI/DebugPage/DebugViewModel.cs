using StinterLogger.RaceLogging.General.Debug;
using StinterLogger.RaceLogging.General.SimEventArgs;
using StinterLogger.UI.Helpers;
using StinterLogger.UI.MainApp;
using System.Collections.Generic;
using System.Windows;

namespace StinterLogger.UI.DebugPage
{
    public class DebugViewModel : ObservableObject, IPageViewModel
    {
        private DebugManager _debugLogger;

        private readonly List<DebugModel> _debugModels;

        public DebugViewModel()
        {
            this.Name = "Debug log";
            this._debugModels = new List<DebugModel>();
            this._debugLogger = ((App)Application.Current).DebugLogger;
            this._debugLogger.OnDataModelChange += this.OnDebugOutput;
        }

        public string Name { get; set; }

        public List<DebugModel> DebugModelList
        {
            get
            {
                return this._debugModels;
            }
        }

        private void AddDebugModel(DebugEventArgs debugEventArgs)
        {
            this._debugModels.Add(
                new DebugModel(debugEventArgs.DebugLog.DateTime,
                debugEventArgs.DebugLog.Description,
                debugEventArgs.DebugLog.DebugLogType,
                debugEventArgs.DebugLog.Index));

            this.OnPropertyChanged("DebugModelList");
        }

        private void OnDebugOutput(object sender, DebugEventArgs debugEventArgs)
        {
            this.AddDebugModel(debugEventArgs);
        }
    }
}
