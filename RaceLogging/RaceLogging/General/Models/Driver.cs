using StinterLogger.RaceLogging.General.Fuel;

namespace StinterLogger.RaceLogging.General.Models
{
    public class Driver
    {
        public int GlobalUserId { get; set; }

        public int LocalId { get; set; }

        public string DriverName { get; set; }

        public string CarNameLong { get; set; }

        public FuelUnit Unit { get; set; }

        public DriverStates CurrentDriverState { get; set; }

        public DriverStates PreviousDriverState { get; set; }
    }
}
