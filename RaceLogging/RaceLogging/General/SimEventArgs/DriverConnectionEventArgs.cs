using StinterLogger.RaceLogging.General.Models;
using System;

namespace StinterLogger.RaceLogging.General.SimEventArgs
{
    public class DriverConnectionEventArgs : EventArgs
    {
        public Driver ActiveDriverInfo { get; set; }
    }
}
