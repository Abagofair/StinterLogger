using System.Collections.Generic;

namespace StinterLogger.RaceLogging.General.Models
{
    public class Lap
    {
        public Lap()
        {
            this.SectorTimes = new List<float>();
        }

        public bool InPit { get; set; }

        public float LapTime { get; set; }

        public int Incidents { get; set; } 

        public float FuelInTankAtFinish { get; set; }

        public float FuelInTankAtStart { get; set; }

        public List<float> SectorTimes { get; set; }

        public int LapNumber { get; set; }

        public int RaceLaps { get; set; }

        public int PlayerCarClassPosition { get; set; }

        public int PlayerCarPosition { get; set; }

        public double RemainingSessionTime { get; set; }
    }
}
