namespace StinterLogger.RaceLogging.General.Models
{
    public class Stint
    {
        public Program Program { get; set; }

        public int LapCount { get; set; }

        public Lap Lap { get; set; }
    }
}
