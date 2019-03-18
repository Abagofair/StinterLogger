using StinterLogger.RaceLogging.General.Program.Data;
using System;

namespace RaceLogging.RaceLogging.General.SimEventArgs
{
    public class ProgramEndEventArgs : EventArgs
    {
        public ProgramData ProgramData { get; set; }
    }
}
