using StinterLogger.RaceLogging.Iracing.Debug;
using StinterLogger.RaceLogging.Iracing.IracingEventArgs;
using StinterLogger.UI.Helpers;
using StinterLogger.UI.MainApp;
using System;
using System.Collections.Generic;
using System.Windows;

namespace StinterLogger.UI.DebugPage
{
    public class DebugViewModel : ObservableObject, IPageViewModel
    {
        private DebugLogger _debugLogger;

        public DebugViewModel()
        {
            this.Name = "Debug log";
            this.DebugModelList = new List<DebugModel>();
            this._debugLogger = ((App)Application.Current).DebugLogger;
            this._debugLogger.OnDataModelChange += this.OnDebugOutput;
        }

        public string Name { get; set; }

        public List<DebugModel> DebugModelList { get; set; }

        private void AddDebugLog(DebugModel debugModel)
        {
            this.DebugModelList.Add(debugModel);
            this.OnPropertyChanged("DebugModelList");
        }

        private void OnDebugOutput(object sender, DebugEventArgs debugEventArgs)
        {
            this.AddDebugLog(
                new DebugModel(debugEventArgs.DateTime, debugEventArgs.Content, debugEventArgs.DebugLogType)
                );
        }
    }
}
