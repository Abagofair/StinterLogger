using StinterLogger.RaceLogging.Iracing.Models;
using System;

namespace StinterLogger.RaceLogging.Iracing.IracingEventArgs
{
    public class FuelDataEventArgs : EventArgs
    {
        public FuelData FuelData { get; set; }
    }
}
