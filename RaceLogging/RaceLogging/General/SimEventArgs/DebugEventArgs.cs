using RaceLogging.General.Debug;
using System;

namespace RaceLogging.General.SimEventArgs
{
    public class DebugEventArgs : EventArgs
    {
        public DebugLog DebugLog { get; set; }
    }
}
