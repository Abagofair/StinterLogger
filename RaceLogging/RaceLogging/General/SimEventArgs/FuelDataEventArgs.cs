using StinterLogger.RaceLogging.General.Fuel;
using System;

namespace StinterLogger.RaceLogging.General.SimEventArgs
{
    public class FuelDataEventArgs : EventArgs
    {
        public FuelManagerData FuelData { get; set; }
    }
}
