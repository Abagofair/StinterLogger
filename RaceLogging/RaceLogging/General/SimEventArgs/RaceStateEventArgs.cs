using RaceLogging.RaceLogging.General.Enums;
using System;

namespace RaceLogging.General.SimEventArgs
{
    public class RaceStateEventArgs : EventArgs
    {
        public RaceState RaceState { get; set; }
    }
}
