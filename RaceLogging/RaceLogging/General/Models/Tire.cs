using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.General.Models
{
    public class Tire
    {
        public float LRwearL { get; set; }
        public float LRwearM { get; set; }
        public float LRwearR { get; set; }

        public float RRwearL { get; set; }
        public float RRwearM { get; set; }
        public float RRwearR { get; set; }

        public float RFwearL { get; set; }
        public float RFwearM { get; set; }
        public float RFwearR { get; set; }

        public float LFwearL { get; set; }
        public float LFwearM { get; set; }
        public float LFwearR { get; set; }
    }
}
