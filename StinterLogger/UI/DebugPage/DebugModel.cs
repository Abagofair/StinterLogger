using RaceLogging.General.Debug;
using StinterLogger.UI.Helpers;
using System;

namespace StinterLogger.UI.DebugPage
{
    public class DebugModel : ObservableObject
    {
        public DebugModel(DateTime time, string description, DebugLogType debugLogType, int index)
        {
            this._dateTime = time;
            this.Description = description;
            this._debugLogType = debugLogType;
            this.Index = index;
        }

        public int Index { get; set; }

        private DateTime _dateTime;
        public string DateTime
        {
            get
            {
                return this._dateTime.ToShortTimeString() + " ";
            }
        }

        public string Description { get; }

        private DebugLogType _debugLogType;
        public string DebugLogType
        {
            get
            {
                return "("+this._debugLogType.ToString()+"): ";
            }
        }
    }
}
