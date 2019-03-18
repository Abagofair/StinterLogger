using StinterLogger.RaceLogging.General.Models;
using System;

namespace StinterLogger.RaceLogging.General.SimEventArgs
{
    public class TelemetryEventArgs : EventArgs
    {
        DateTime RealDateTimeRecieved { get; set; }

        public Telemetry Telemetry { get; set; }
    }
}
