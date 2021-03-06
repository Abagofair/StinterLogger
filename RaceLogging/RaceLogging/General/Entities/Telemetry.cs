﻿namespace RaceLogging.General.Entities
{
    public class Telemetry
    {
        public float FuelInTank { get; set; }

        public float Speed { get; set; }

        public float BrakePressurePct { get; set; }

        public float ThrottlePressurePct { get; set; }

        public float Rpm { get; set; }

        public float LapDistancePct { get; set; }

        public int Gear { get; set; }
    }
}
