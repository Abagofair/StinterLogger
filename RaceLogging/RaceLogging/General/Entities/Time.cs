using System.Collections.Generic;

namespace RaceLogging.General.Entities
{
    public class Time
    {
        public Time()
        {
            this.SectorTimes = new List<float>();
        }

        public float LapTime { get; set; }

        public List<float> SectorTimes { get; set; }
    }
}
