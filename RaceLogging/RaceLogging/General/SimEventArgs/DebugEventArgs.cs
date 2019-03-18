using StinterLogger.RaceLogging.General.Debug;
using System;

namespace StinterLogger.RaceLogging.General.SimEventArgs
{
    public class DebugEventArgs : EventArgs
    {
        public DebugLog DebugLog { get; set; }
    }
}
