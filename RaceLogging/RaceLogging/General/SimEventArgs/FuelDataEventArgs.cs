using RaceLogging.General.Fuel;
using System;

namespace RaceLogging.General.SimEventArgs
{
    public class FuelDataEventArgs : EventArgs
    {
        public FuelManagerData FuelData { get; set; }
    }
}
