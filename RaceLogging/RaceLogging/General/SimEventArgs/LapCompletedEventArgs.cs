using RaceLogging.General.Entities;
using System;

namespace RaceLogging.General.SimEventArgs
{
    public class LapCompletedEventArgs : EventArgs
    {
        public Lap Lap { get; set; }
    }
}
