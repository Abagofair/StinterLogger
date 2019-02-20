using StinterLogger.RaceLogging.Iracing.Models;
using System;

namespace StinterLogger.RaceLogging.Iracing.IracingEventArgs
{
    public class LapCompletedEventArgs : EventArgs
    {
        public TelemetryLapData TelemetryLapData { get; set; }
    }
}
