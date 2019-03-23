using RaceLogging.General.Program.Config;
using System.Collections.Generic;

namespace RaceLogging.General.Entities
{
    public class SimProgram
    {
        public SimProgram()
        {
            this.CompletedLaps = new List<Lap>();
        }

        public string SetupName { get; set; }

        public ProgramConfig ProgramConfig { get; set; }

        public List<Lap> CompletedLaps { get; set; } 

        public Driver Driver { get; set; }
    }
}
