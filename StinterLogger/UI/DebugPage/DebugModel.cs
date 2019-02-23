using StinterLogger.RaceLogging.Iracing.Debug;
using StinterLogger.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.UI.DebugPage
{
    public class DebugModel : ObservableObject
    {
        public DebugModel(DateTime time, string content, DebugLogType debugLogType)
        {
            this._dateTime = time;
            this.Content = content;
            this._debugLogType = debugLogType;
        }

        private DateTime _dateTime;
        public string DateTime
        {
            get
            {
                return this._dateTime.ToShortTimeString();
            }
        }

        public string Content { get; }

        private DebugLogType _debugLogType;
        public string DebugLogType
        {
            get
            {
                return this._debugLogType.ToString();
            }
        }
    }
}
