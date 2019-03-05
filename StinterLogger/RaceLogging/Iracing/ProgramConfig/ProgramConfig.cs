using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.ProgramConfig
{
    public class ProgramConfig
    {
        public ProgramConfig()
        {
            this.Data = new Data();
        }

        public string Name { get; set; }

        public float TelemetryUpdateFrequency { get; set; }

        public int StintCount { get; set; }

        public int LapsPerStint { get; set; }

        public Data Data { get; set; }
    }
}
