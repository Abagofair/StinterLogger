using System;
using System.Collections.Generic;
using System.Text;

namespace RaceLogging.RaceLogging.General.Entities
{
    public class ProgramDebrief
    {
        public ProgramDebrief(int sectors)
        {
            this.AvgSectorTimes = new float[sectors];
        }

        public float AvgLapTime { get; set; }

        public float[] AvgSectorTimes { get; set; }

        public float AvgLFWearPrLap { get; set; }

        public float AvgRFWearPrLap { get; set; }

        public float AvgLRWearPrLap { get; set; }

        public float AvgRRWearPrLap { get; set; }

        public float AvgFuelUsagePrLap { get; set; }

        public double MaxPitDeltaWithStall { get; set; }
    }
}
