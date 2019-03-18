using StinterLogger.RaceLogging.General.Models;
using System.Collections.Generic;

namespace StinterLogger.RaceLogging.General.Program.Data
{
    public class LapTelemetry
    {
        public LapTelemetry()
        {
            this.Telemetries = new List<Telemetry>();
        }

        public Lap CompletedLap { get; set; }

        public List<Telemetry> Telemetries { get; }
    }
}
