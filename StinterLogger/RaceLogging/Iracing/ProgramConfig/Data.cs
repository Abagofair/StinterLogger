using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing.ProgramConfig
{
    public class Data
    {
        public Data()
        {
            this.Track = new Dictionary<string, bool>();
            this.Telemetry = new Dictionary<string, bool>();
            this.Time = new Dictionary<string, bool>();
        }

        public Dictionary<string, bool> Track { get; set; }

        public Dictionary<string, bool> Telemetry { get; set; }

        public Dictionary<string, bool> Time { get; set; }
    }
}
