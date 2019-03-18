using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.General.Models
{
    public class Program
    {
        public string Name { get; set; }

        public string Setup { get; set; }

        public int ExpectedStintCount { get; set; }

        public int ExpectedLapCount { get; set; }

        public Track Track { get; set; }
    }
}
