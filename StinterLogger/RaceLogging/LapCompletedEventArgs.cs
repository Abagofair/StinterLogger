using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging
{
    public class LapCompletedEventArgs : EventArgs
    {
        public TelemetryLapData TelemetryLapData { get; set; }
    }
}
