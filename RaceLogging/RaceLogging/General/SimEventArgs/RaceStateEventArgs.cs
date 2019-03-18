using StinterLogger.RaceLogging.General.Models;
using System;

namespace StinterLogger.RaceLogging.General.SimEventArgs
{
    public class RaceStateEventArgs : EventArgs
    {
        public RaceState RaceState { get; set; }
    }
}
