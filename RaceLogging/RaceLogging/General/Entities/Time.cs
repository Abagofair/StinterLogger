using System.Collections.Generic;

namespace RaceLogging.General.Entities
{
    public class Time
    {
        public float LapTime { get; set; }

        public List<float> SectorTimes { get; set; }
    }
}
