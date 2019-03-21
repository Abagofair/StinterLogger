using StinterLogger.RaceLogging.General.Program.Data;
using System;

namespace StinterLogger.RaceLogging.General.SimEventArgs
{
    public class ProgramEndEventArgs : EventArgs
    {
        public ProgramData ProgramData { get; set; }
    }
}
