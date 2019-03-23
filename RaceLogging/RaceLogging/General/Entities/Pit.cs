namespace RaceLogging.General.Entities
{
    public class Pit
    {
        public bool WasInStall { get; set; }

        public double PitDeltaSeconds { get; set; }

        public Tire Tire { get; set; }
    }
}
