using System.Collections.Generic;

namespace RaceLogging.General.Program.Config
{
    public enum Condition
    {
        InPitStall, Minutes, Laps, FreeRoam
    }

    public class EndCondition
    {
        public Condition Condition { get; set; }
        public int Count { get; set; }
    }

    public class ProgramConfig
    {
        public ProgramConfig()
        {
            this.Telemetry = new List<string>();
            this.EndCondition = new EndCondition();
        }

        public string Name { get; set; }

        public double TelemetryUpdateFrequency { get; set; }

        public bool LogPitDelta { get; set; }

        public bool LogTireWear { get; set; }

        public EndCondition EndCondition { get; }

        public List<string> Telemetry { get; set; }
    }
}
