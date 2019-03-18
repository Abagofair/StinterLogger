using StinterLogger.RaceLogging.General.Models;
using StinterLogger.RaceLogging.General.Program.Config;
using System.Collections.Generic;

namespace StinterLogger.RaceLogging.General.Program.Data
{
    public class ProgramData
    {
        public ProgramData()
        {
            this.Tires = new List<Tire>();
            this.PitDeltas = new List<Pit>();
            this.LapData = new List<LapTelemetry>();
        }

        public ProgramConfig ProgramConfig { get; set; }

        public Track Track { get; set; }

        public Driver Driver { get; set; }

        public List<LapTelemetry> LapData { get; set; }

        public List<Tire> Tires { get; set; }

        public List<Pit> PitDeltas { get; set; }
    }
}
