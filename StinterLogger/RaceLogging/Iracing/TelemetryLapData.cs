using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing
{
    public class TelemetryLapData
    {
        public TelemetryLapData()
        {
            this.Readings = new List<OnTrackReading>();
        }

        public bool IsCompleted { get; set; }

        public float LapTime { get; set; }

        public List<float> SectorTimes { get; set; }

        public int LapNumber { get; set; }

        public double RemainingSessionTime { get; set; }

        public List<OnTrackReading> Readings { get; set; }
    }
}
