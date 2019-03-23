using RaceLogging.General.Entities;
using System;

namespace StinterLogger.RaceLogging.General.SimEventArgs
{
    public class TelemetryEventArgs : EventArgs
    {
        public Telemetry Telemetry { get; set; }
    }
}
