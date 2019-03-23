using System.Collections.Generic;

namespace RaceLogging.General.Entities
{
    public class Lap
    {
        public Lap()
        {
            this.Time = new Time();
            this.Pit = null;
            this.Driver = new Driver();
            this.Track = new Track();
            this.Car = new Car();
            this.Telemetry = new List<Telemetry>();
        }

        public Time Time { get; set; }

        public Pit Pit { get; set; }

        public Driver Driver { get; set; }

        public Track Track { get; set; }

        public Car Car { get; set; }

        public List<Telemetry> Telemetry { get; set; }

        public int Incidents { get; set; }

        public float FuelInTankAtFinish { get; set; }

        public float FuelInTankAtStart { get; set; }

        public float FuelUsed { get => System.Math.Abs(this.FuelInTankAtStart - this.FuelInTankAtFinish); }

        public float TrackTemp { get; set; }

        public float AirTemp { get; set; }

        public int LapNumber { get; set; }

        public float RemainingSessionTime { get; set; }
    }
}
