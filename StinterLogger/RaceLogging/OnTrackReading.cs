using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging
{
    public class OnTrackReading
    {
        public float FuelLevel { get; set; }

        public int Incident { get; set; }

        public float Speed { get; set; }

        public float Brake { get; set; }

        public float Throttle { get; set; }

        public float Rpm { get; set; }

        public float LapDistancePct { get; set; }

        public int Gear { get; set; }

        public float OilTemperature { get; set; }

        public float TrackTempCrew { get; set; }

        public double UpdateTime { get; set; }
    }
}
