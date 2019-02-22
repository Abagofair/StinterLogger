using StinterLogger.RaceLogging.Iracing.Fuel;
using System;

namespace StinterLogger.RaceLogging.Iracing.IracingEventArgs
{
    public class FuelDataEventArgs : EventArgs
    {
        public FuelManagerData FuelData { get; set; }
    }
}
