using StinterLogger.RaceLogging.General.Models;
using System;

namespace StinterLogger.RaceLogging.General.SimEventArgs
{
    public class LapCompletedEventArgs : EventArgs
    {
        public Lap Lap { get; set; }
    }
}
