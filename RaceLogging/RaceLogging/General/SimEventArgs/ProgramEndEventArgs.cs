using RaceLogging.General.Entities;
using System;

namespace RaceLogging.General.SimEventArgs
{
    public class ProgramEndEventArgs : EventArgs
    {
        public SimProgram SimProgram { get; set; }
    }
}