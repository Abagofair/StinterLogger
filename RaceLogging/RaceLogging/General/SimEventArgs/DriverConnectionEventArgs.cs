using RaceLogging.General.Entities;
using System;

namespace StinterLogger.RaceLogging.General.SimEventArgs
{
    public class DriverConnectionEventArgs : EventArgs
    {
        public Driver CurrentDriver { get; set; }
    }
}
