using RaceLogging.General.Entities;
using System;

namespace RaceLogging.General.SimEventArgs
{
    public class TelemetryEventArgs : EventArgs
    {
        public Telemetry Telemetry { get; set; }
    }
}
