using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.General.Models
{
    public class Sector
    {
        public int Number { get; set; }

        public float PctOfTrack { get; set; }

        public float MetersUntilSectorEnd { get; set; }
    }
}
